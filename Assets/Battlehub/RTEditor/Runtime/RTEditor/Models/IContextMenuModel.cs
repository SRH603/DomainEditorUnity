using Battlehub.RTCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor.Models
{
    public class ContextMenuItem
    {
        public class ValidationArgs
        {
            public bool IsValid;
        }

        public Action<string> Action;
        public Action<ValidationArgs> Validate;

        public virtual int TypeIndex
        {
            get;
            set;
        }

        public virtual string Path
        {
            get;
            set;
        }

        public virtual string Text
        {
            get;
            set;
        }

        public virtual Sprite Icon
        {
            get;
            set;
        }
        public virtual string Command
        {
            get;
            set;
        }
    }

    public class ContextMenuAnchor
    {
        public object Target
        {
            get;
            private set;
        }

        public object[] Selection
        {
            get;
            private set;
        }

        public T GetTarget<T>()
        {
            if (Target is T)
            {
                return (T)Target;
            }
            return default;
        }

        public T[] GetSelection<T>()
        {
            return Selection.OfType<T>().ToArray();
        }

        public ContextMenuAnchor(object target, object[] selection)
        {
            Target = target;
            Selection = selection != null ? selection : new object[0];
        }
    }


    public abstract class ContextMenuArgs : EventArgs
    {
        public abstract string WindowName { get; }

        public abstract ContextMenuAnchor Anchor { get; }

        public abstract IReadOnlyList<ContextMenuItem> Items { get; }

        public abstract ContextMenuItem AddMenuItem(string path, Action<string> action, Action<ContextMenuItem.ValidationArgs> validate = null);

        public abstract ContextMenuItem InsertMenuItem(int index, string path, Action<string> action, Action<ContextMenuItem.ValidationArgs> validate = null);

        public abstract void RemoveMenuItem(int index);

        public abstract void ClearMenuItems();
    }

    public class DefaultContextMenuArgs : ContextMenuArgs
    {
        private List<ContextMenuItem> m_menuItems;

        public override IReadOnlyList<ContextMenuItem> Items
        {
            get { return m_menuItems; }
        }

        private string m_windowName;
        public override string WindowName
        {
            get { return m_windowName; }
        }

        private ContextMenuAnchor m_anchor;
        public override ContextMenuAnchor Anchor
        {
            get { return m_anchor; }
        }

        public DefaultContextMenuArgs(string windowName, List<ContextMenuItem> menuItems)
        {
            m_windowName = windowName;
            m_menuItems = menuItems;
        }

        public DefaultContextMenuArgs(string viewName, ContextMenuAnchor anchor, List<ContextMenuItem> menuItems)
        {
            m_windowName = viewName;
            m_menuItems = menuItems;

            m_anchor = anchor;
        }

        public override ContextMenuItem AddMenuItem(string path, Action<string> action, Action<ContextMenuItem.ValidationArgs> validate = null)
        {
            var menuItem = new ContextMenuItem
            {
                Path = path,
                Action = action,
                Validate = validate
            };
            m_menuItems.Add(menuItem);
            return menuItem;
        }

        public override ContextMenuItem InsertMenuItem(int index, string path, Action<string> action, Action<ContextMenuItem.ValidationArgs> validate = null)
        {
            var menuItem = new ContextMenuItem
            {
                Path = path,
                Action = action,
                Validate = validate
            };
            m_menuItems.Insert(index, menuItem);
            return menuItem;
        }

        public override void RemoveMenuItem(int index)
        {
            m_menuItems.RemoveAt(index);
        }

        public override void ClearMenuItems()
        {
            m_menuItems.Clear();
        }
    }

    public interface IContextMenuModel
    {
        event EventHandler<ContextMenuArgs> Open;

        event EventHandler Close;

        void RaiseOpen(ContextMenuArgs args);

        void Show();

        void Show(params ContextMenuItem[] items);

        void Show(ContextMenuArgs args);
    }

    public class ContextMenuModel : IContextMenuModel, IDisposable
    {
        public event EventHandler<ContextMenuArgs> Open;
        public event EventHandler Close;

        private IContextMenu m_contextMenu;
        public ContextMenuModel()
        {
            m_contextMenu = IOC.Resolve<IContextMenu>();
            m_contextMenu.Closed += OnClosed;
        }

        public void Dispose()
        {
            m_contextMenu.Closed -= OnClosed;
            m_contextMenu = null;
        }

        private void OnClosed(object sender, EventArgs e)
        {
            Close?.Invoke(this, EventArgs.Empty);
        }

        public void RaiseOpen(ContextMenuArgs args)
        {
            Open?.Invoke(this, args);
        }

        public void Show()
        {
            Show(new ContextMenuItem[0]);
        }

        public void Show(params ContextMenuItem[] items)
        {
            var wm = IOC.Resolve<IWindowManager>();
            var pointerOverWindow = wm.FindPointerOverWindow(null);
            var windowName = string.Empty;
            if (pointerOverWindow != null)
            {
                var window = pointerOverWindow.GetComponentInParent<RuntimeWindow>();
                if (window != null)
                {
                    windowName = window.name;
                }
            }

            var sourceItemsList = new List<ContextMenuItem>(items);
            var args = new DefaultContextMenuArgs(windowName, sourceItemsList);
            Show(args);
        }

        public void Show(ContextMenuArgs args)
        {
            RaiseOpen(args);

            var targetItems = new List<UIControls.MenuControl.MenuItemInfo>();
            foreach (var sourceItem in args.Items)
            {
                var menuItem = new UIControls.MenuControl.MenuItemInfo
                {
                    Path = sourceItem.Path,
                    Text = sourceItem.Text,
                    Icon = sourceItem.Icon,
                    PrefabIndex = sourceItem.TypeIndex,

                    Command = sourceItem.Command,
                    Action = new UIControls.MenuControl.MenuItemEvent(),
                    Validate = new UIControls.MenuControl.MenuItemValidationEvent(),
                };

                ContextMenuItem.ValidationArgs sourceArgs = new ContextMenuItem.ValidationArgs();
                menuItem.Action.AddListener(arg => sourceItem.Action?.Invoke(arg));
                menuItem.Validate.AddListener(targetArgs =>
                {
                    sourceArgs.IsValid = true;
                    sourceItem.Validate?.Invoke(sourceArgs);
                    targetArgs.IsValid = sourceArgs.IsValid;
                });

                targetItems.Add(menuItem);
            }

            m_contextMenu.Open(targetItems.ToArray());
        }
    }
}

