using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Tunetoon.Accounts;

namespace Tunetoon.Login
{
    public class LoginHandlerBase<T> : ILoginHandler<T> where T : Account
    {
        public List<T> AccountsToTwoStepAuth = new List<T>();

        public virtual async Task RequestLogin(BindingList<T> accountList)
        {
            AccountsToTwoStepAuth.Clear();

            // Separar contas com prioridade (login sequencial) das sem prioridade (paralelo)
            var prioritized = new List<T>();
            var normal = new List<T>();

            foreach (var account in accountList)
            {
                if (!account.CanLogin()) continue;
                if (account.LoginPriority > 0)
                    prioritized.Add(account);
                else
                    normal.Add(account);
            }

            // Ordenar por prioridade
            prioritized.Sort((a, b) => a.LoginPriority.CompareTo(b.LoginPriority));

            // Login sequencial para contas priorizadas
            foreach (var account in prioritized)
                await Task.Run(() => GetAuthResponse(account));

            // Login paralelo para as demais
            var tasks = new List<Task>();
            foreach (var account in normal)
                tasks.Add(Task.Run(() => GetAuthResponse(account)));
            await Task.WhenAll(tasks);
        }

        public virtual async Task HandleTwoStep()
        {
            await Task.CompletedTask;
        }

        public async Task LoginAccounts(BindingList<T> accountList)
        {
            await RequestLogin(accountList);
            await HandleTwoStep();
        }

        public virtual void GetAuthResponse(T account)
        {
            throw new NotImplementedException();
        }

        public virtual void HandleAuthResponse(T account)
        {
            throw new NotImplementedException();
        }
    }
}