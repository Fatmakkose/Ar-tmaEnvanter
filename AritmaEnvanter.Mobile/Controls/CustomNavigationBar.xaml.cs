using Microsoft.Maui.Controls;

namespace AritmaEnvanter.Mobile.Controls;

public partial class CustomNavigationBar : ContentView
{
    public static readonly BindableProperty CurrentPageProperty =
        BindableProperty.Create(nameof(CurrentPage), typeof(string), typeof(CustomNavigationBar), string.Empty, propertyChanged: OnCurrentPageChanged);

    public string CurrentPage
    {
        get => (string)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    private Color _selectedColor = Color.FromArgb("#2E67D1");
    private Color _unselectedColor = Color.FromArgb("#7C8FAE");

    public CustomNavigationBar()
    {
        InitializeComponent();
    }

    private static void OnCurrentPageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CustomNavigationBar control)
        {
            control.UpdateSelection(newValue as string ?? string.Empty);
        }
    }

    private async void OnItemTapped(object sender, TappedEventArgs e)
    {
        var route = e.Parameter as string;
        if (string.IsNullOrEmpty(route) || route == CurrentPage) return;

        // Simple scale effect on tap
        if (sender is Grid grid)
        {
            await grid.ScaleTo(0.95, 50);
            await grid.ScaleTo(1.0, 50);
        }

        await Shell.Current.GoToAsync($"//{route}");
    }

    private void UpdateSelection(string route)
    {
        // Reset all
        ResetItem(ProductsIndicator, ProductsContainer, ProductsIcon, ProductsText);
        ResetItem(MovementsIndicator, MovementsContainer, MovementsIcon, MovementsText);
        ResetItem(RequestsIndicator, RequestsContainer, RequestsIcon, RequestsText);

        // Select active
        switch (route)
        {
            case "Stoklar":
                SelectItem(ProductsIndicator, ProductsContainer, ProductsIcon, ProductsText);
                break;
            case "Hareketler":
                SelectItem(MovementsIndicator, MovementsContainer, MovementsIcon, MovementsText);
                break;
            case "Talepler":
                SelectItem(RequestsIndicator, RequestsContainer, RequestsIcon, RequestsText);
                break;
        }
    }

    private void ResetItem(BoxView indicator, Border container, Microsoft.Maui.Controls.Shapes.Path icon, Label text)
    {
        indicator.IsVisible = false;
        container.BackgroundColor = Colors.Transparent;
        container.Shadow = new Shadow
        {
            Opacity = 0f,
            Radius = 0,
            Offset = new Point(0, 0)
        };
        icon.Fill = new SolidColorBrush(_unselectedColor);
        text.TextColor = _unselectedColor;
        text.FontAttributes = FontAttributes.None;
    }

    private void SelectItem(BoxView indicator, Border container, Microsoft.Maui.Controls.Shapes.Path icon, Label text)
    {
        indicator.IsVisible = true;
        container.BackgroundColor = Color.FromArgb("#EAF2FF");
        container.Shadow = new Shadow
        {
            Brush = new SolidColorBrush(Color.FromArgb("#9EC0F8")),
            Opacity = 0.4f,
            Radius = 10,
            Offset = new Point(0, 2)
        };
        icon.Fill = new SolidColorBrush(_selectedColor);
        text.TextColor = _selectedColor;
        text.FontAttributes = FontAttributes.Bold;
        
        // Optional: Animate indicator
        indicator.Opacity = 0;
        indicator.FadeTo(1, 200);
    }
}
