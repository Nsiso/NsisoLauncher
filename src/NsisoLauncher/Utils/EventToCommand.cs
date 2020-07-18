using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace NsisoLauncher.Utils
{
    //this code is from https://github.com/yue-fei/csharp-mvvm

    #region EventToCommand

    /// <summary>
    ///     This <see cref="T:System.Windows.Interactivity.TriggerAction" /> can be
    ///     used to bind any event on any FrameworkElement to an <see cref="T:System.Windows.Input.ICommand" />.
    ///     Typically, this element is used in XAML to connect the attached element
    ///     to a command located in a ViewModel. This trigger can only be attached
    ///     to a FrameworkElement or a class deriving from FrameworkElement.
    ///     <para>
    ///         To access the EventArgs of the fired event, use a RelayCommand&lt;EventArgs&gt;
    ///         and leave the CommandParameter and CommandParameterValue empty!
    ///     </para>
    /// </summary>
    public class EventToCommand : TriggerAction<DependencyObject>
    {
        /// <summary>
        ///     Identifies the <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.CommandParameter" /> dependency property
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter", typeof(object), typeof(EventToCommand), new PropertyMetadata(null,
                delegate(DependencyObject s, DependencyPropertyChangedEventArgs e)
                {
                    var eventToCommand = s as EventToCommand;
                    if (eventToCommand == null) return;
                    if (eventToCommand.AssociatedObject == null) return;
                    eventToCommand.EnableDisableElement();
                }));

        /// <summary>
        ///     Identifies the <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.Command" /> dependency property
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
            typeof(ICommand), typeof(EventToCommand),
            new PropertyMetadata(null,
                delegate(DependencyObject s, DependencyPropertyChangedEventArgs e)
                {
                    OnCommandChanged(s as EventToCommand, e);
                }));

        /// <summary>
        ///     Identifies the <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.MustToggleIsEnabled" /> dependency property
        /// </summary>
        public static readonly DependencyProperty MustToggleIsEnabledProperty = DependencyProperty.Register(
            "MustToggleIsEnabled", typeof(bool), typeof(EventToCommand), new PropertyMetadata(false,
                delegate(DependencyObject s, DependencyPropertyChangedEventArgs e)
                {
                    var eventToCommand = s as EventToCommand;
                    if (eventToCommand == null) return;
                    if (eventToCommand.AssociatedObject == null) return;
                    eventToCommand.EnableDisableElement();
                }));

        private object _commandParameterValue;
        private bool? _mustToggleValue;

        /// <summary>
        ///     Gets or sets the ICommand that this trigger is bound to. This
        ///     is a DependencyProperty.
        /// </summary>
        public ICommand Command
        {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>
        ///     Gets or sets an object that will be passed to the
        ///     <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.Command" />
        ///     attached to this trigger. This is a DependencyProperty.
        /// </summary>
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        /// <summary>
        ///     Gets or sets an object that will be passed to the
        ///     <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.Command" />
        ///     attached to this trigger. This property is here for compatibility
        ///     with the Silverlight version. This is NOT a DependencyProperty.
        ///     For databinding, use the <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.CommandParameter" /> property.
        /// </summary>
        public object CommandParameterValue
        {
            get => _commandParameterValue ?? CommandParameter;
            set
            {
                _commandParameterValue = value;
                EnableDisableElement();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the attached element must be
        ///     disabled when the <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.Command" /> property's CanExecuteChanged
        ///     event fires. If this property is true, and the command's CanExecute
        ///     method returns false, the element will be disabled. If this property
        ///     is false, the element will not be disabled when the command's
        ///     CanExecute method changes. This is a DependencyProperty.
        /// </summary>
        public bool MustToggleIsEnabled
        {
            get => (bool) GetValue(MustToggleIsEnabledProperty);
            set => SetValue(MustToggleIsEnabledProperty, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the attached element must be
        ///     disabled when the <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.Command" /> property's CanExecuteChanged
        ///     event fires. If this property is true, and the command's CanExecute
        ///     method returns false, the element will be disabled. This property is here for
        ///     compatibility with the Silverlight version. This is NOT a DependencyProperty.
        ///     For databinding, use the <see cref="P:GalaSoft.MvvmLight.Command.EventToCommand.MustToggleIsEnabled" /> property.
        /// </summary>
        public bool MustToggleIsEnabledValue
        {
            get
            {
                if (_mustToggleValue.HasValue) return _mustToggleValue.Value;
                return MustToggleIsEnabled;
            }
            set
            {
                _mustToggleValue = value;
                EnableDisableElement();
            }
        }

        /// <summary>
        ///     Specifies whether the EventArgs of the event that triggered this
        ///     action should be passed to the bound RelayCommand. If this is true,
        ///     the command should accept arguments of the corresponding
        ///     type (for example RelayCommand&lt;MouseButtonEventArgs&gt;).
        /// </summary>
        public bool PassEventArgsToCommand { get; set; }

        /// <summary>
        ///     Called when this trigger is attached to a FrameworkElement.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            EnableDisableElement();
        }

        private Control GetAssociatedObject()
        {
            return AssociatedObject as Control;
        }

        /// <summary>
        ///     This method is here for compatibility
        ///     with the Silverlight 3 version.
        /// </summary>
        /// <returns>
        ///     The command that must be executed when
        ///     this trigger is invoked.
        /// </returns>
        private ICommand GetCommand()
        {
            return Command;
        }

        /// <summary>
        ///     Provides a simple way to invoke this trigger programatically
        ///     without any EventArgs.
        /// </summary>
        public void Invoke()
        {
            Invoke(null);
        }

        /// <summary>
        ///     Executes the trigger.
        ///     <para>
        ///         To access the EventArgs of the fired event, use a RelayCommand&lt;EventArgs&gt;
        ///         and leave the CommandParameter and CommandParameterValue empty!
        ///     </para>
        /// </summary>
        /// <param name="parameter">The EventArgs of the fired event.</param>
        protected override void Invoke(object parameter)
        {
            if (AssociatedElementIsDisabled()) return;
            var command = GetCommand();
            var obj = CommandParameterValue;
            if (obj == null && PassEventArgsToCommand) obj = parameter;
            if (command != null && command.CanExecute(obj)) command.Execute(obj);
        }

        private static void OnCommandChanged(EventToCommand element, DependencyPropertyChangedEventArgs e)
        {
            if (element == null) return;
            if (e.OldValue != null) ((ICommand) e.OldValue).CanExecuteChanged -= element.OnCommandCanExecuteChanged;
            var command = (ICommand) e.NewValue;
            if (command != null) command.CanExecuteChanged += element.OnCommandCanExecuteChanged;
            element.EnableDisableElement();
        }

        private bool AssociatedElementIsDisabled()
        {
            var associatedObject = GetAssociatedObject();
            return AssociatedObject == null || associatedObject != null && !associatedObject.IsEnabled;
        }

        private void EnableDisableElement()
        {
            var associatedObject = GetAssociatedObject();
            if (associatedObject == null) return;
            var command = GetCommand();
            if (MustToggleIsEnabledValue && command != null)
                associatedObject.IsEnabled = command.CanExecute(CommandParameterValue);
        }

        private void OnCommandCanExecuteChanged(object sender, EventArgs e)
        {
            EnableDisableElement();
        }
    }

    #endregion
}