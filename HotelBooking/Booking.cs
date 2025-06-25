using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace HotelBooking
{
    public class Booking : INotifyPropertyChanged
    {
        public static readonly Dictionary<string, decimal> RoomPrices = new Dictionary<string, decimal>
        {
            { "Одноместный", 2000 },
            { "Двухместный", 3000 },
            { "Люкс", 5000 },
            { "Делюкс", 8000 }
        };

        public Guid Guid { get; set; } = Guid.NewGuid();

        private string _fullName = "";
        public string FullName
        {
            get => _fullName;
            set { _fullName = value ?? ""; OnPropertyChanged(nameof(FullName)); }
        }

        private string _roomType = "Одноместный";
        public string RoomType
        {
            get => _roomType;
            set
            {
                _roomType = value ?? "Одноместный";
                OnPropertyChanged(nameof(RoomType));
                OnPropertyChanged(nameof(RoomPricePerDay));
            }
        }

        [JsonIgnore]
        public string RoomPricePerDay => RoomPrices.TryGetValue(RoomType, out decimal price)
            ? $"{price} руб./сутки"
            : "Цена не установлена";

        public DateTime CheckInDate { get; set; } = DateTime.Today;
        public DateTime CheckOutDate { get; set; } = DateTime.Today.AddDays(1);

        private string _imageBase64 = "";
        public string ImageBase64
        {
            get => _imageBase64;
            set { _imageBase64 = value ?? ""; OnPropertyChanged(nameof(Image)); }
        }

        [JsonIgnore]
        public BitmapImage Image
        {
            get
            {
                if (string.IsNullOrEmpty(ImageBase64))
                    return null;

                try
                {
                    var imageBytes = Convert.FromBase64String(ImageBase64);
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = new MemoryStream(imageBytes);
                    image.EndInit();
                    image.Freeze();
                    return image;
                }
                catch
                {
                    return null;
                }
            }
        }

        public decimal TotalPrice { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}