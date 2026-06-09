using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Tunetoon.Forms
{
    public static class SosDetector
    {
        // Região da cabeça/chapéu do NPC na gaiola
        // Calibrada visualmente na captura real (310x1017) com todos os 29 NPCs
        // Menor diff entre qualquer par = 0.193, zero colisões
        private const double HeadX1 = 0.4516;   // 140/310
        private const double HeadY1 = 0.0344;   // 35/1017
        private const double HeadX2 = 0.5484;   // 170/310
        private const double HeadY2 = 0.0590;   // 60/1017

        private static string SosFolder => 
            System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName),
                "SOS");

        // Tamanho padrão das referências — toda captura é redimensionada para isso
        // antes de extrair o crop, normalizando diferenças de resolução entre máquinas
        private const int RefWidth  = 310;
        private const int RefHeight = 1017;

        // Cache das assinaturas de referência — calculadas uma vez ao iniciar
        private static float[][] _refSignatures = null;
        private static SosCard[] _refCards = null;

        public static void PreloadReferences()
        {
            if (!Directory.Exists(SosFolder)) return;
            var sigs  = new System.Collections.Generic.List<float[]>();
            var cards = new System.Collections.Generic.List<SosCard>();
            foreach (var card in SosCardData.All)
            {
                string path = Path.Combine(SosFolder, card.Name + ".png");
                if (!File.Exists(path)) continue;
                try
                {
                    using var refBmp = new Bitmap(path);
                    var rect = MakeRect(refBmp.Width, refBmp.Height, HeadX1, HeadY1, HeadX2, HeadY2);
                    using var refCrop = refBmp.Clone(rect, PixelFormat.Format32bppArgb);
                    sigs.Add(ExtractNormalizedTemplate(refCrop));
                    cards.Add(card);
                }
                catch { }
            }
            _refSignatures = sigs.ToArray();
            _refCards      = cards.ToArray();
        }

        // Threshold: metade da menor separação entre qualquer par (0.193/2 ≈ 0.09)
        private const float MatchThreshold = 0.20f;

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        public static SosCard Detect(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return null;
            try
            {
                GetClientRect(hWnd, out RECT clientRect);
                var pt = new POINT { X = 0, Y = 0 };
                ClientToScreen(hWnd, ref pt);

                int w = clientRect.Right - clientRect.Left;
                int h = clientRect.Bottom - clientRect.Top;
                if (w <= 0 || h <= 0) return null;

                using var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(bmp))
                    g.CopyFromScreen(pt.X, pt.Y, 0, 0, new Size(w, h));

                // DEBUG
                bmp.Save(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "sos_full.png"), ImageFormat.Png);

                // Redimensionar para tamanho padrão das referências
                // Isso normaliza diferenças de resolução entre máquinas
                using var normalized = new Bitmap(bmp, RefWidth, RefHeight);

                var headRect = MakeRect(RefWidth, RefHeight, HeadX1, HeadY1, HeadX2, HeadY2);
                using var headCrop = normalized.Clone(headRect, PixelFormat.Format32bppArgb);
                headCrop.Save(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "sos_head.png"), ImageFormat.Png);

                float[] sig = ExtractNormalizedTemplate(headCrop);
                return FindBestMatch(sig);
            }
            catch (Exception ex)
            {
                File.WriteAllText(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "sos_error.txt"), ex.ToString());
                return null;
            }
        }

        private static Rectangle MakeRect(int w, int h,
            double x1r, double y1r, double x2r, double y2r)
        {
            int x  = (int)(w * x1r);
            int y  = (int)(h * y1r);
            int rw = Math.Max(1, (int)(w * (x2r - x1r)));
            int rh = Math.Max(1, (int)(h * (y2r - y1r)));
            return new Rectangle(x, y, rw, rh);
        }

        // Extrai pixels normalizados do crop para template matching
        // Invariante a variações de brilho entre máquinas
        private static float[] ExtractNormalizedTemplate(Bitmap bmp)
        {
            byte[] buf = LockBitsToBytes(bmp);
            int n = buf.Length / 4;
            var pixels = new float[n * 3];

            // Extrair RGB
            float sumR = 0, sumG = 0, sumB = 0;
            for (int i = 0, j = 0; i < buf.Length; i += 4, j++)
            {
                float b = buf[i], g = buf[i+1], r = buf[i+2];
                pixels[j*3]   = r;
                pixels[j*3+1] = g;
                pixels[j*3+2] = b;
                sumR += r; sumG += g; sumB += b;
            }

            // Normalizar (média 0, desvio 1) — invariante a brilho absoluto
            float mean = (sumR + sumG + sumB) / (n * 3);
            float variance = 0;
            foreach (var v in pixels) variance += (v - mean) * (v - mean);
            float std = (float)Math.Sqrt(variance / pixels.Length) + 1e-6f;

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = (pixels[i] - mean) / std;

            return pixels;
        }

        private static byte[] LockBitsToBytes(Bitmap bmp)
        {
            var data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int bytes = Math.Abs(data.Stride) * bmp.Height;
            byte[] buf = new byte[bytes];
            Marshal.Copy(data.Scan0, buf, 0, bytes);
            bmp.UnlockBits(data);
            return buf;
        }

        private static SosCard FindBestMatch(float[] sig)
        {
            // Usar cache pré-calculado para velocidade máxima
            if (_refSignatures == null || _refCards == null)
                PreloadReferences();
            if (_refSignatures == null) return null;

            SosCard best = null;
            float bestDiff = float.MaxValue;
            var log = new System.Text.StringBuilder();

            for (int i = 0; i < _refSignatures.Length; i++)
            {
                float diff = ComputeL1(sig, _refSignatures[i]);
                log.AppendLine($"{_refCards[i].Name,-25}: {diff:F4}");
                if (diff < bestDiff) { bestDiff = diff; best = _refCards[i]; }
            }

            log.AppendLine($"\nMelhor: {best?.Name ?? "nenhum"} diff={bestDiff:F4} threshold={MatchThreshold}");
            File.WriteAllText(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "sos_match.txt"), log.ToString());

            return bestDiff < MatchThreshold ? best : null;
        }

        private static float ComputeL1(float[] a, float[] b)
        {
            int len = Math.Min(a.Length, b.Length);
            float diff = 0;
            for (int i = 0; i < len; i++)
                diff += Math.Abs(a[i] - b[i]);
            return diff / len;
        }
    }

    public class SosResultForm : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out WinRECT rect);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct WinRECT { public int Left, Top, Right, Bottom; }

        private static readonly Dictionary<string, int> _confirmCount = new();
        private readonly IntPtr _gameHWnd;

        public static void ResetConfirmation() => _confirmCount.Clear();

        public SosResultForm(List<(string ToonName, SosCard Card)> results, IntPtr gameHWnd = default)
        {
            _gameHWnd = gameHWnd;
            Text            = "SOS Card Check";
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition   = FormStartPosition.Manual;
            AutoSize        = true;
            AutoSizeMode    = AutoSizeMode.GrowAndShrink;
            Padding         = new Padding(12);
            BackColor       = Color.FromArgb(30, 30, 30);
            ForeColor       = Color.White;
            Font            = new Font("Segoe UI", 10f);
            TopMost         = true;

            SosCard detectedCard = null;
            foreach (var (_, card) in results)
                if (card != null) { detectedCard = card; break; }

            if (detectedCard != null)
            {
                if (!_confirmCount.ContainsKey(detectedCard.Name))
                    _confirmCount[detectedCard.Name] = 0;
                _confirmCount[detectedCard.Name]++;
            }

            int count = detectedCard != null && _confirmCount.TryGetValue(detectedCard.Name, out int c) ? c : 0;

            var layout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize      = true,
                AutoSizeMode  = AutoSizeMode.GrowAndShrink,
                WrapContents  = false,
                BackColor     = Color.Transparent,
            };

            if (detectedCard == null)
            {
                layout.Controls.Add(new Label
                {
                    Text      = "No SOS Card identified.\nMake sure the game is in Shopping layout.",
                    AutoSize  = true,
                    ForeColor = Color.LightGray,
                });
            }
            else
            {
                string stars = new string('★', detectedCard.Stars) + new string('☆', 5 - detectedCard.Stars);
                bool confirmed  = count >= 2;
                Color textColor = confirmed ? Color.LimeGreen : Color.Red;
                string statusLine = confirmed ? "Confirmed (2/2)" : $"Confirming... ({count}/2)";
                if (confirmed) _confirmCount.Clear();

                bool hideGag = detectedCard.Ability is "Cogs Miss" or "Toons Hit" or "Restock";
                string midLine = hideGag
                    ? $"  {detectedCard.Ability}  |  {detectedCard.Track}"
                    : $"  {detectedCard.Ability}  |  {detectedCard.Track}  |  {detectedCard.Gag}";
                string infoText = $"{detectedCard.Name}  {stars}\n" +
                                  midLine + "\n" +
                                  $"  {detectedCard.Effect}";

                var panel = new Panel
                {
                    AutoSize     = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Padding      = new Padding(8),
                    BackColor    = Color.FromArgb(45, 45, 45),
                };
                var inner = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.TopDown,
                    AutoSize      = true,
                    AutoSizeMode  = AutoSizeMode.GrowAndShrink,
                    WrapContents  = false,
                    BackColor     = Color.Transparent,
                };
                inner.Controls.Add(new Label { Text = infoText, AutoSize = true, ForeColor = Color.White });
                inner.Controls.Add(new Label { Text = statusLine, AutoSize = true, ForeColor = textColor });
                panel.Controls.Add(inner);
                layout.Controls.Add(panel);
            }

            Controls.Add(layout);

            // Posiciona sempre ao lado direito da posição Shopping do monitor atual,
            // independente do layout atual das janelas do jogo
            this.Load += (s, ev) =>
            {
                Screen screen = _gameHWnd != IntPtr.Zero
                    ? Screen.FromHandle(_gameHWnd)
                    : Screen.PrimaryScreen;

                Rectangle wa = screen.WorkingArea;

                // Calcula o retângulo Shopping exato igual ao WindowManager
                int shopX = wa.X + (int)(wa.Width * 0.419);
                int shopW = (int)(wa.Width * 0.1625);
                int shopY = wa.Y;

                this.Location = new Point(shopX + shopW + 8, shopY);
            };
        }

        public bool AddResult(List<(string ToonName, SosCard Card)> results)
        {
            SosCard detectedCard = null;
            foreach (var (_, card) in results)
                if (card != null) { detectedCard = card; break; }

            if (detectedCard == null) return false;

            if (!_confirmCount.ContainsKey(detectedCard.Name))
                _confirmCount[detectedCard.Name] = 0;
            _confirmCount[detectedCard.Name]++;
            int count = _confirmCount[detectedCard.Name];

            string stars = new string('★', detectedCard.Stars) + new string('☆', 5 - detectedCard.Stars);
            bool isConfirmed = count >= 2;
            Color textColor   = isConfirmed ? Color.LimeGreen : Color.Red;
            string statusText = isConfirmed ? "Confirmed (2/2)" : $"Confirming... ({count}/2)";

            var layout = (FlowLayoutPanel)Controls[0];

            var panel = new Panel
            {
                AutoSize     = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding      = new Padding(8, 6, 8, 6),
                Margin       = new Padding(0, 0, 0, 4),
                BackColor    = Color.FromArgb(45, 45, 45),
            };

            var inner = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize      = true,
                AutoSizeMode  = AutoSizeMode.GrowAndShrink,
                WrapContents  = false,
                BackColor     = Color.Transparent,
            };

            if (count == 1)
            {
                inner.Controls.Add(new Label { Text = $"{detectedCard.Name}  {stars}", AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 10f, FontStyle.Bold) });
                bool hideGag2 = detectedCard.Ability is "Cogs Miss" or "Toons Hit" or "Restock";
                string midLine2 = hideGag2
                    ? $"{detectedCard.Ability}  |  {detectedCard.Track}"
                    : $"{detectedCard.Ability}  |  {detectedCard.Track}  |  {detectedCard.Gag}";
                inner.Controls.Add(new Label { Text = midLine2, AutoSize = true, ForeColor = Color.White });
                inner.Controls.Add(new Label { Text = detectedCard.Effect, AutoSize = true, ForeColor = Color.White });
            }

            inner.Controls.Add(new Label { Text = statusText, AutoSize = true, ForeColor = textColor });
            panel.Controls.Add(inner);
            layout.Controls.Add(panel);

            if (!isConfirmed) return false;

            // Copia Nome, Estrelas, Gag e Efeito para o clipboard ao confirmar (2/2)
            string clipLabel = detectedCard.Ability switch
            {
                "Heal"    => "Toon-Up",
                "Attack"  => detectedCard.Track, // Trap, Sound ou Drop
                "Restock" => $"Restock {detectedCard.Track}",
                _         => detectedCard.Ability
            };
            string clipText = $"{detectedCard.Name}  {clipLabel}  {detectedCard.Stars} Stars";
            try
            {
                // Garante execução na thread UI (STA) para o Clipboard funcionar
                if (System.Threading.Thread.CurrentThread.GetApartmentState() == System.Threading.ApartmentState.STA)
                {
                    Clipboard.SetText(clipText);
                }
                else
                {
                    var t = new System.Threading.Thread(() => Clipboard.SetText(clipText));
                    t.SetApartmentState(System.Threading.ApartmentState.STA);
                    t.Start();
                    t.Join();
                }
            } catch { }

            _confirmCount.Clear();
            var timer = new System.Windows.Forms.Timer { Interval = 12000 };
            timer.Tick += (s, ev) => { timer.Stop(); this.Close(); };
            timer.Start();
            return true;
        }
    }
}
