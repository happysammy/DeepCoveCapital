using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DeepCoveCapital.Core
{
    public partial class PricePoint
    {
        public PricePoint(string unixTime, string price, string amount)
        {
            _Time = UnixTimeSecondsToDateTime(unixTime);
            _Price = decimal.Parse(price);
            _Amount = double.Parse(amount);
        }

        public PricePoint(DateTimeOffset time, decimal price, double amount)
        {
            _Time = time;
            _Price = price;
            _Amount = amount;
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0,
                                                      DateTimeKind.Utc);

        public static DateTimeOffset UnixTimeSecondsToDateTime(string text)
        {
            double seconds = double.Parse(text, CultureInfo.InvariantCulture);
            return Epoch.AddSeconds(seconds);
        }


        public static DateTimeOffset UnixTimeNanoSecondsToDateTime(string text)
        {
            long nanoseconds = long.Parse(text, CultureInfo.InvariantCulture);
            return Epoch.AddSeconds(nanoseconds / 1000000000);
        }

        public static long DateTimeToUnixTimeNonoseconds(DateTimeOffset date)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch

            //TEMP MODIF
            TimeSpan span = (date - Epoch);

            //return the total nono seconds (which is a UNIX timestamp for kraken)
            return (long)span.TotalSeconds * 1000000000;
        }
    }
    public partial class PricePoint : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged
    {

        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private double _AmountField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private decimal _PriceField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.DateTimeOffset _TimeField;

        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public double _Amount
        {
            get
            {
                return this._AmountField;
            }
            set
            {
                if ((this._AmountField.Equals(value) != true))
                {
                    this._AmountField = value;
                    this.RaisePropertyChanged("_Amount");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public decimal _Price
        {
            get
            {
                return this._PriceField;
            }
            set
            {
                if ((this._PriceField.Equals(value) != true))
                {
                    this._PriceField = value;
                    this.RaisePropertyChanged("_Price");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.DateTimeOffset _Time
        {
            get
            {
                return this._TimeField;
            }
            set
            {
                if ((this._TimeField.Equals(value) != true))
                {
                    this._TimeField = value;
                    this.RaisePropertyChanged("_Time");
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }

}
