using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace DeepCoveCapital.Infrastructure
{
    /// <summary>
    /// Encapsulates INotifyPropertyChanged implementation for View Models and Services to use
    /// </summary>
    public class ObservableObject : INotifyPropertyChanged
    {

        static readonly Mediator mediator = new Mediator();

        public Mediator Mediator
        {
            get { return mediator; }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression">property</param>
        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (PropertyChanged == null)
            {
                return;
            }

            var body = propertyExpression.Body as MemberExpression;
            if (body != null)
            {
                var property = body.Member as PropertyInfo;
                if (property != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property.Name));
                }
            }

        }
    }
}
