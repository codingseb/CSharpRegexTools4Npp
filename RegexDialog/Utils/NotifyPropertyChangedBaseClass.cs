using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RegexDialog
{
    /// <summary>
    /// Base class that implements INotifyPropertyChanged and some useful method to fire PropertyChanged event
    /// </summary>
    public class NotifyPropertyChangedBaseClass : INotifyPropertyChanged
    {
        /// <summary>
        /// Fired when the value of a property has been modified.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Génerate the event PropertyChanged for the specified propertyName.
        /// If no property name specified ti automatically take the name of the property who call this method.
        /// To notify changes on other property think to use nameof for better Refactoring rename management.
        /// </summary>
        /// <param name="propertyName">The name of the property of which we want ot nofify the change</param>
        public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
