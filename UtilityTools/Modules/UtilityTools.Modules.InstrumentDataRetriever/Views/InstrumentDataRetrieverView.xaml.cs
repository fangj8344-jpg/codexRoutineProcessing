using System.Windows;
using System.Windows.Controls;

namespace UtilityTools.Modules.InstrumentDataRetriever.Views
{
    public partial class InstrumentDataRetrieverView : UserControl
    {
        public InstrumentDataRetrieverView()
        {
            InitializeComponent();
        }

        private void StartDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DatePicker datePicker && datePicker.SelectedDate.HasValue)
            {
                var viewModel = DataContext as ViewModels.InstrumentDataRetrieverViewModel;
                if (viewModel != null)
                {
                    viewModel.StartTime = datePicker.SelectedDate.Value;
                }
            }
        }

        private void EndDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DatePicker datePicker && datePicker.SelectedDate.HasValue)
            {
                var viewModel = DataContext as ViewModels.InstrumentDataRetrieverViewModel;
                if (viewModel != null)
                {
                    viewModel.EndTime = datePicker.SelectedDate.Value;
                }
            }
        }
    }
} 