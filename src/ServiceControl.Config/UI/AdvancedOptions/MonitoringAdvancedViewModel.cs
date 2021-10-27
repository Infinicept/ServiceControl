﻿namespace ServiceControl.Config.UI.AdvancedOptions
{
    using System.Windows.Input;
    using Caliburn.Micro;
    using Commands;
    using Events;
    using Framework;
    using Framework.Rx;
    using ServiceControlInstaller.Engine.Instances;

    class MonitoringAdvancedViewModel : RxProgressScreen
    {
        public MonitoringAdvancedViewModel(BaseService instance, IEventAggregator eventAggregator, DeleteMonitoringlnstanceCommand deleteInstanceCommand)
        {
            MonitoringInstance = (MonitoringInstance)instance;
            DisplayName = "ADVANCED OPTIONS";

            DeleteCommand = deleteInstanceCommand;

            CopyToClipboard = new CopyToClipboardCommand();

            Cancel = Command.Create(async () =>
            {
                TryClose(false);
                await eventAggregator.PublishOnUIThreadAsync(new RefreshInstances());
            }, () => !InProgress);
        }

        public MonitoringInstance MonitoringInstance { get; }

        public ICommand DeleteCommand { get; set; }

        public ICommand Cancel { get; set; }


        public ICommand CopyToClipboard { get; private set; }

        public string Name => MonitoringInstance.Name;
    }
}