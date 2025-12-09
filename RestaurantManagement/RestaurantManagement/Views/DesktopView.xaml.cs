using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Toolkit.Popup;

namespace RestaurantManagement;

public partial class DesktopView : ContentPage
{
	public DesktopView()
	{
		InitializeComponent();
	}

    private void ImageButton_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button)
        {
            var popup = new SfPopup
            {
                ShowFooter = false,
                ShowHeader = false,
                WidthRequest = 130,
                HeightRequest = 55,
                AutoCloseDuration = 1300,
                Background = Colors.Transparent,
                PopupStyle = new PopupStyle
                {
                    CornerRadius = 8,
                    PopupBackground = Colors.Transparent
                },

                ContentTemplate = new DataTemplate(() =>
                {
                    var stackLayout = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Spacing = 2,
                        Background = Color.FromArgb("#E5F5EA"),
                    };

                    var label1 = new Label
                    {
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        TextColor = Color.FromArgb("#4c554d"),
                        FontSize = 11,
                        FontFamily = "DeliusUnicase",

                        FormattedText = new FormattedString
                        {
                            Spans =
                            {
                                new Span  {Text = "Open : ", FontSize=14, FontAttributes = FontAttributes.Bold},
                                new Span  { Text = "7.30 AM", FontSize= 12}
                            }
                        },
                    };

                    var label2 = new Label
                    {
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        TextColor = Color.FromArgb("#4c554d"),
                        FontSize = 11,
                        FontFamily = "DeliusUnicase",

                        FormattedText = new FormattedString
                        {
                            Spans =
                            {
                                new Span  {Text = "Close : ", FontSize=14, FontAttributes = FontAttributes.Bold},
                                new Span  { Text = "12 PM", FontSize= 12}
                            }
                        },
                    };

                    stackLayout.Children.Add(label1);
                    stackLayout.Children.Add(label2);

                    var border = new Border
                    {
                        Stroke = Color.FromArgb("#0091cf"),
                        StrokeThickness = 2,
                        BackgroundColor = Color.FromArgb("#dff4ff"),
                        Padding = 5,
                        StrokeShape = new RoundRectangle { CornerRadius = 8 },
                        Content = stackLayout
                    };

                    return border;
                })
            };

            popup.ShowRelativeToView(time, PopupRelativePosition.AlignBottom);
        }
    }
}