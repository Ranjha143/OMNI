using PluginManager;
using Quartz;
using System.ComponentModel;

namespace RetailPro2_X
{
    internal class GenerateRPAuth : IJob
    {
        readonly BackgroundWorker threadWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        public GenerateRPAuth()
        {
            threadWorker.DoWork += ThreadWorker_DoWork;
            threadWorker.ProgressChanged += ThreadWorker_ProgressChanged;
            threadWorker.RunWorkerCompleted += ThreadWorker_RunWorkerCompleted;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Delay(0);
            threadWorker.RunWorkerAsync();
        }
        private void ThreadWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var workerTask = Task.Factory.StartNew(() => ProccessQueue().Wait());
            Task.WaitAll(workerTask);
        }
        private void ThreadWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {

        }
        private void ThreadWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {

        }
        private async Task<bool> ProccessQueue()
        {

            try
            {
                await Task.Delay(0);
                
                GlobalVariables.RetailProAuthSession = await RetailProAuthentication.GetSession(GlobalVariables.RProConfig.PrismUser, GlobalVariables.RProConfig.PrismPassword, "webclient");
            }
            catch (Exception)
            {

            }
            return true;
        }

    }

}
