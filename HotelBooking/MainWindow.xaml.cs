using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace HotelBooking
{
    public partial class MainWindow : Window
    {
        private const string DataFilePath = "bookings.json";
        public ObservableCollection<Booking> Bookings { get; } = new ObservableCollection<Booking>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBookings();
            UpdatePricePreview();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePricePreview();
        }

        private void cmbRoomType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePricePreview();
        }

        private void UpdatePricePreview()
        {
            try
            {
                if (cmbRoomType == null || txtRoomPrice == null || txtPricePreview == null)
                    return;

                string roomType = cmbRoomType.SelectedItem is ComboBoxItem item
                    ? item.Content?.ToString()
                    : "Одноместный";

                if (string.IsNullOrEmpty(roomType))
                    roomType = "Одноместный";

                decimal pricePerDay = Booking.RoomPrices.TryGetValue(roomType, out decimal price)
                    ? price
                    : 2000;

                txtRoomPrice.Text = $"{pricePerDay} руб./сутки";

                if (dpCheckIn?.SelectedDate != null &&
                    dpCheckOut?.SelectedDate != null &&
                    dpCheckIn.SelectedDate.Value < dpCheckOut.SelectedDate.Value)
                {
                    int days = (int)(dpCheckOut.SelectedDate.Value - dpCheckIn.SelectedDate.Value).TotalDays;
                    if (days > 0)
                    {
                        txtPricePreview.Text = $"Предварительная стоимость: {days * pricePerDay} руб.";
                        return;
                    }
                }

                txtPricePreview.Text = $"Цена за сутки: {pricePerDay} руб.";
            }
            catch (Exception ex)
            {
                if (txtRoomPrice != null) txtRoomPrice.Text = "Ошибка расчета";
                if (txtPricePreview != null) txtPricePreview.Text = ex.Message;
            }
        }

        private void btnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp|Все файлы|*.*"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    byte[] imageBytes = File.ReadAllBytes(openFileDialog.FileName);
                    string base64String = Convert.ToBase64String(imageBytes);

                    imgPreview.Source = new BitmapImage(new Uri(openFileDialog.FileName));

                    imgPreview.Tag = base64String;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBook_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtFullName.Text))
                {
                    MessageBox.Show("Введите ФИО клиента",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!dpCheckIn.SelectedDate.HasValue || !dpCheckOut.SelectedDate.HasValue)
                {
                    MessageBox.Show("Выберите даты заезда/выезда",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime checkIn = dpCheckIn.SelectedDate.Value.Date;
                DateTime checkOut = dpCheckOut.SelectedDate.Value.Date;

                if (checkIn >= checkOut)
                {
                    MessageBox.Show("Дата выезда должна быть позже даты заезда",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string roomType = "Одноместный";
                if (cmbRoomType.SelectedItem is ComboBoxItem selectedItem &&
                    selectedItem.Content != null)
                {
                    roomType = selectedItem.Content.ToString();
                }

                decimal pricePerDay = Booking.RoomPrices.TryGetValue(roomType, out decimal price)
                    ? price
                    : 2000;

                int days = (int)(checkOut - checkIn).TotalDays;
                if (days < 1) days = 1;

                decimal totalPrice = days * pricePerDay;

                var booking = new Booking
                {
                    FullName = txtFullName.Text.Trim(),
                    RoomType = roomType,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    ImageBase64 = imgPreview.Tag as string ?? "",
                    TotalPrice = totalPrice
                };

                Bookings.Add(booking);
                SaveBookings();
                ClearForm();

                MessageBox.Show($"Бронирование успешно создано!\nСумма к оплате: {totalPrice} руб.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании бронирования: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void LoadBookings()
        {
            try
            {
                if (File.Exists(DataFilePath))
                {
                    string json = File.ReadAllText(DataFilePath);
                    var loadedBookings = JsonConvert.DeserializeObject<ObservableCollection<Booking>>(json)
                        ?? new ObservableCollection<Booking>();

                    Bookings.Clear();
                    foreach (var booking in loadedBookings)
                    {
                        Bookings.Add(booking);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveBookings()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Bookings, Formatting.Indented);
                File.WriteAllText(DataFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            if (txtFullName != null) txtFullName.Clear();

            if (cmbRoomType != null && cmbRoomType.Items.Count > 0)
                cmbRoomType.SelectedIndex = 0;

            if (dpCheckIn != null) dpCheckIn.SelectedDate = DateTime.Today;
            if (dpCheckOut != null) dpCheckOut.SelectedDate = DateTime.Today.AddDays(1);

            if (imgPreview != null)
            {
                imgPreview.Source = null;
                imgPreview.Tag = null;
            }

            if (txtRoomPrice != null) txtRoomPrice.Text = "";
            if (txtPricePreview != null) txtPricePreview.Text = "";
        }
    }
}