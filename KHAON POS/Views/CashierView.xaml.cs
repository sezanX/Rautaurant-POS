using System;
using System.Windows;
using System.Windows.Controls;

namespace KHAONPOS.Views;

public partial class CashierView : UserControl
{
    public static readonly DependencyProperty ColumnsCountProperty =
        DependencyProperty.Register(nameof(ColumnsCount), typeof(int), typeof(CashierView), new PropertyMetadata(4));

    public int ColumnsCount
    {
        get => (int)GetValue(ColumnsCountProperty);
        set => SetValue(ColumnsCountProperty, value);
    }

    public CashierView()
    {
        InitializeComponent();
        this.SizeChanged += CashierView_SizeChanged;
    }

    private void CashierView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateColumns();
    }

    private void UpdateColumns()
    {
        double availableWidth = this.ActualWidth - 350 - 44;
        if (availableWidth <= 0) return;

        int cols = (int)Math.Floor(availableWidth / 250);
        if (cols < 1) cols = 1;

        ColumnsCount = cols;
    }
}
