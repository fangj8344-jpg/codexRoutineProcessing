using Moq;
using Prism.Ioc;
using Prism.Regions;
using UtilityTools.Core.Dialog;
using UtilityTools.Modules.NetController.ViewModels;
using UtilityTools.Services.Interfaces;
using Xunit;

namespace UtilityTools.Modules.ModuleName.Tests.ViewModels
{
    public class ViewAViewModelFixture
    {
        Mock<IDialogHostService> _dialogHostServiceMock;
        Mock<IContainerProvider> _containerProviderMock;
        const string MessageServiceDefaultMessage = "Some Value";

        public ViewAViewModelFixture()
        {
            var dialogHostService = new Mock<IDialogHostService>();
            //dialogHostService.Setup(x => x.ShowDialog()).Returns(MessageServiceDefaultMessage);
            _dialogHostServiceMock = dialogHostService;

            _containerProviderMock = new Mock<IContainerProvider>();
        }

        [Fact]
        public void MessagePropertyValueUpdated()
        {
            var vm = new NetControllerViewModel(_containerProviderMock.Object, _dialogHostServiceMock.Object);

            //_dialogHostServiceMock.Verify(x => x.GetMessage(), Times.Once);

            Assert.Equal(MessageServiceDefaultMessage, vm.Message);
        }

        [Fact]
        public void MessageINotifyPropertyChangedCalled()
        {
            var vm = new NetControllerViewModel(_containerProviderMock.Object, _dialogHostServiceMock.Object);
            Assert.PropertyChanged(vm, nameof(vm.Message), () => vm.Message = "Changed");
        }
    }
}
