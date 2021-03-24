using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace GetLogsClient.Commands
{
    public abstract class CommandBase<T> : MarkupExtension, ICommand
           where T : class, ICommand, new()
    {
        private static T _command;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _command ?? (_command = new T());
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public abstract void Execute(object parameter);

        public virtual bool CanExecute(object parameter)
        {
            return !IsDesignMode;
        }


        public static bool IsDesignMode =>
            (bool)
            DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty,
                    typeof(FrameworkElement))
                .Metadata.DefaultValue;


        public static T TryFindParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            DependencyObject parentObject = GetParentObject(child);

            switch (parentObject)
            {
                case null:
                    return null;
                case T parent:
                    return parent;
                default:
                    return TryFindParent<T>(parentObject);
            }
        }

        public static DependencyObject GetParentObject(DependencyObject child)
        {
            switch (child)
            {
                case null:
                    return null;
                case ContentElement contentElement:
                {
                    DependencyObject parent = ContentOperations.GetParent(contentElement);
                    if (parent != null) return parent;

                    return contentElement is FrameworkContentElement fce ? fce.Parent : null;
                }

                default:
                    return VisualTreeHelper.GetParent(child);
            }
        }
    }
}