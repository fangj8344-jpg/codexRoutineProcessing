using Moq;
using Prism.Ioc;
using Prism.Regions;
using UtilityTools.Modules.NetController.ViewModels;
using UtilityTools.Services.Interfaces;
using Xunit;

namespace UtilityTools.Modules.ModuleName.Tests.ViewModels
{
    public class ViewAViewModelFixture
    {
        Mock<IMessageService> _messageServiceMock;
        Mock<IContainerProvider> _containerProviderMock;
        const string MessageServiceDefaultMessage = "Some Value";

        public ViewAViewModelFixture()
        {
            var messageService = new Mock<IMessageService>();
            messageService.Setup(x => x.GetMessage()).Returns(MessageServiceDefaultMessage);
            _messageServiceMock = messageService;

            _containerProviderMock = new Mock<IContainerProvider>();
        }

        [Fact]
        public void MessagePropertyValueUpdated()
        {
            var vm = new NetControllerViewModel(_containerProviderMock.Object, _messageServiceMock.Object);

            _messageServiceMock.Verify(x => x.GetMessage(), Times.Once);

            Assert.Equal(MessageServiceDefaultMessage, vm.Message);
        }

        [Fact]
        public void MessageINotifyPropertyChangedCalled()
        {
            var vm = new NetControllerViewModel(_containerProviderMock.Object, _messageServiceMock.Object);
            Assert.PropertyChanged(vm, nameof(vm.Message), () => vm.Message = "Changed");
        }
    }
}
