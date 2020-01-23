using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace NsisoLauncher.Utils
{
    //this code is from https://github.com/yue-fei/csharp-mvvm

    #region  EventToCommand
    /// <summary>
    /// This <see cref="T:System.Windows.Interactivity.TriggerAction" /> can be
    /// used to bind any event on any FrameworkElement to an <see cref="T:System.Windows.Input.ICommand" />.
    /// Typically, this element is used in XAML to connect the attached element
    /// to a command located in a ViewModel. This trigger can only be attached
    /// to a FrameworkElement or a class deriving from FrameworkElement.
    /// <para>To access the EventArgs of the fired event, use a RelayCommand&lt;EventArgs&gt;
    /// and leave the CommandParameter and CommandParameterValue empty!</para>
    /// </summary>
    public class EventToCommand : TriggerAction<DependencyObject>
    {
        /// <summary>
        /// Identifies the <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.CommandParameter" /> dependency property
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(EventToCommand), new PropertyMetadata(null, delegate (DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            EventToCommand eventToCommand = s as EventToCommand;
            if (eventToCommand == null)
            {
                return;
            }
            if (eventToCommand.AssociatedObject == null)
            {
                return;
            }
            eventToCommand.EnableDisableElement();
        }));
        /// <summary>
        /// Identifies the <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.Command" /> dependency property
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(EventToCommand), new PropertyMetadata(null, delegate (DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            EventToCommand.OnCommandChanged(s as EventToCommand, e);
        }));
        /// <summary>
        /// Identifies the <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.MustToggleIsEnabled" /> dependency property
        /// </summary>
        public static readonly DependencyProperty MustToggleIsEnabledProperty = DependencyProperty.Register("MustToggleIsEnabled", typeof(bool), typeof(EventToCommand), new PropertyMetadata(false, delegate (DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            EventToCommand eventToCommand = s as EventToCommand;
            if (eventToCommand == null)
            {
                return;
            }
            if (eventToCommand.AssociatedObject == null)
            {
                return;
            }
            eventToCommand.EnableDisableElement();
        }));
        private object _commandParameterValue;
        private bool? _mustToggleValue;
        /// <summary>
        /// Gets or sets the ICommand that this trigger is bound to. This
        /// is a DependencyProperty.
        /// </summary>
        public ICommand Command
        {
            get
            {
                return (ICommand)base.GetValue(EventToCommand.CommandProperty);
            }
            set
            {
                base.SetValue(EventToCommand.CommandProperty, value);
            }
        }
        /// <summary>
        /// Gets or sets an object that will be passed to the <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.Command" />
        /// attached to this trigger. This is a DependencyProperty.
        /// </summary>
        public object CommandParameter
        {
            get
            {
                return base.GetValue(EventToCommand.CommandParameterProperty);
            }
            set
            {
                base.SetValue(EventToCommand.CommandParameterProperty, value);
            }
        }
        /// <summary>
        /// Gets or sets an object that will be passed to the <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.Command" />
        /// attached to this trigger. This property is here for compatibility
        /// with the Silverlight version. This is NOT a DependencyProperty.
        /// For databinding, use the <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.CommandParameter" /> property.
        /// </summary>
        public object CommandParameterValue
        {
            get
            {
                return this._commandParameterValue ?? this.CommandParameter;
            }
            set
            {
                this._commandParameterValue = value;
                this.EnableDisableElement();
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether the attached element must be
        /// disabled when the <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.Command" /> property's CanExecuteChanged
        /// event fires. If this property is true, and the command's CanExecute 
        /// method returns false, the element will be disabled. If this property
        /// is false, the element will not be disabled when the command's
        /// CanExecute method changes. This is a DependencyProperty.
        /// </summary>
        public bool MustToggleIsEnabled
        {
            get
            {
                return (bool)base.GetValue(EventToCommand.MustToggleIsEnabledProperty);
            }
            set
            {
                base.SetValue(EventToCommand.MustToggleIsEnabledProperty, value);
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether the attached element must be
        /// disabled when the <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.Command" /> property's CanExecuteChanged
        /// event fires. If this property is true, and the command's CanExecute 
        /// method returns false, the element will be disabled. This property is here for
        /// compatibility with the Silverlight version. This is NOT a DependencyProperty.
        /// For databinding, use the <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.MustToggleIsEnabled" /> property.
        /// </summary>
        public bool MustToggleIsEnabledValue
        {
            get
            {
                if (this._mustToggleValue.HasValue)
                {
                    return this._mustToggleValue.Value;
                }
                return this.MustToggleIsEnabled;
            }
            set
            {
                this._mustToggleValue = new bool?(value);
                this.EnableDisableElement();
            }
        }
        /// <summary>
        /// Specifies whether the EventArgs of the event that triggered this
        /// action should be passed to the bound RelayCommand. If this is true,
        /// the command should accept arguments of the corresponding
        /// type (for example RelayCommand&lt;MouseButtonEventArgs&gt;).
        /// </summary>
        public bool PassEventArgsToCommand
        {
            get;
            set;
        }
        /// <summary>
        /// Called when this trigger is attached to a FrameworkElement.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.EnableDisableElement();
        }
        private Control GetAssociatedObject()
        {
            return base.AssociatedObject as Control;
        }
        /// <summary>
        /// This method is here for compatibility
        /// with the Silverlight 3 version.
        /// </summary>
        /// <returns>The command that must be executed when
        /// this trigger is invoked.</returns>
        private ICommand GetCommand()
        {
            return this.Command;
        }
        /// <summary>
        /// Provides a simple way to invoke this trigger programatically
        /// without any EventArgs.
        /// </summary>
        public void Invoke()
        {
            this.Invoke(null);
        }
        /// <summary>
        /// Executes the trigger.
        /// <para>To access the EventArgs of the fired event, use a RelayCommand&lt;EventArgs&gt;
        /// and leave the CommandParameter and CommandParameterValue empty!</para>
        /// </summary>
        /// <param name="parameter">The EventArgs of the fired event.</param>
        protected override void Invoke(object parameter)
        {
            if (this.AssociatedElementIsDisabled())
            {
                return;
            }
            ICommand command = this.GetCommand();
            object obj = this.CommandParameterValue;
            if (obj == null && this.PassEventArgsToCommand)
            {
                obj = parameter;
            }
            if (command != null && command.CanExecute(obj))
            {
                command.Execute(obj);
            }
        }
        private static void OnCommandChanged(EventToCommand element, DependencyPropertyChangedEventArgs e)
        {
            if (element == null)
            {
                return;
            }
            if (e.OldValue != null)
            {
                ((ICommand)e.OldValue).CanExecuteChanged -= new EventHandler(element.OnCommandCanExecuteChanged);
            }
            ICommand command = (ICommand)e.NewValue;
            if (command != null)
            {
                command.CanExecuteChanged += new EventHandler(element.OnCommandCanExecuteChanged);
            }
            element.EnableDisableElement();
        }
        private bool AssociatedElementIsDisabled()
        {
            Control associatedObject = this.GetAssociatedObject();
            return base.AssociatedObject == null || (associatedObject != null && !associatedObject.IsEnabled);
        }
        private void EnableDisableElement()
        {
            Control associatedObject = this.GetAssociatedObject();
            if (associatedObject == null)
            {
                return;
            }
            ICommand command = this.GetCommand();
            if (this.MustToggleIsEnabledValue && command != null)
            {
                associatedObject.IsEnabled = command.CanExecute(this.CommandParameterValue);
            }
        }
        private void OnCommandCanExecuteChanged(object sender, EventArgs e)
        {
            this.EnableDisableElement();
        }
    }
    #endregion
}
