using Battlehub.RTCommon;
using Battlehub.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public interface ICustomTypeDescriptor
    {
        Type Type { get; }

        PropertyDescriptor[] GetProperties();
    }

    public abstract class CustomTypeDescriptorBase<TCustomType> : ICustomTypeDescriptor
    {
        public Type Type => typeof(TCustomType);

        public virtual PropertyDescriptor[] GetProperties()
        {
            return new PropertyDescriptor[0];
        }

        protected PropertyDescriptor Property<T,U>(string name, Expression<Func<T, U>> expression, object propertMetadata = null)
        {
            return new PropertyDescriptor(name, Strong.MemberInfo(expression))
            {
                PropertyMetadata = propertMetadata,
            };
        }

        protected PropertyDescriptor Method<T>(string name, Expression<Func<T, Delegate>> expression)
        {
            return new PropertyDescriptor(name, Strong.MethodInfo(expression));
        }

        protected PropertyDescriptor Method<T>(string name, Expression<Action<T>> expression)
        {
            return new PropertyDescriptor(name, Strong.MethodInfo(expression));
        }
    }

    public class CustomTypeEditor : PropertyEditor<object>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Transform Panel = null;
        [SerializeField]
        private Toggle Expander = null;
        public bool StartExpanded;
        private IEditorsMap m_editorsMap;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_editorsMap = IOC.Resolve<IEditorsMap>();
            Expander.onValueChanged.AddListener(OnExpanded);
        }

        protected override void StartOverride()
        {
            base.StartOverride();
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (Expander != null)
            {
                Expander.onValueChanged.RemoveListener(OnExpanded);
            }

            if (m_coExpand != null)
            {
                StopCoroutine(m_coExpand);
                m_coExpand = null;
            }
        }

        protected override void SetIndent(float indent)
        {
            RectTransform rt = Expander.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.offsetMin = new Vector2(indent, rt.offsetMin.y);
            }
        }

        protected override void InitOverride(object[] targets, object[] accessors, MemberInfo memberInfo, Action<object, object> eraseTargetCallback, string label = null)
        {
            base.InitOverride(targets, accessors, memberInfo, eraseTargetCallback, label);
           
            FieldInfo[] serializableFields = Reflection.GetSerializableFields(memberInfo.GetType(), false);

            if (StartExpanded)
            {
                Expander.isOn = serializableFields.Length < 8;
            }
        }

        private void OnExpanded(bool isExpanded)
        {
            Panel.gameObject.SetActive(isExpanded);

            CurrentValue = GetValue();
            if (isExpanded)
            {
                CreateElementEditors(CurrentValue);
            }
            else
            {
                foreach (Transform c in Panel)
                {
                    Destroy(c.gameObject);
                }
            }
        }

        private void BuildEditor()
        {
            foreach (Transform c in Panel)
            {
                Destroy(c.gameObject);
            }

            CreateElementEditors(CurrentValue);
        }

        private bool HasPropertyEditorOrSerializable(Type type)
        {
            bool hasPropertyEditor = m_editorsMap.IsPropertyEditorEnabled(type, false);
            if (!hasPropertyEditor && type.IsEnum)
            {
                hasPropertyEditor = m_editorsMap.HasMapping(typeof(Enum));
            }

            return hasPropertyEditor || type.IsDefined(typeof(SerializableAttribute));
        }

        private void CreateElementEditors(object value)
        {
            var memberInfoType = value != null ? value.GetType() : MemberInfoType;

            ICollection<PropertyDescriptor> propertyDescriptors = null;
            if (ChildDescriptors != null)
            {
                propertyDescriptors = ChildDescriptors.Values;
            }
            else if (m_editorsMap.CustomTypeDescriptors.TryGetValue(memberInfoType, out var descriptor))
            {
                propertyDescriptors = descriptor.GetProperties();
            }
            else
            {
                var fields = Reflection.GetSerializableFields(memberInfoType, false);
                propertyDescriptors = new List<PropertyDescriptor>();

                for (int i = 0; i < fields.Length; ++i)
                {
                    FieldInfo fieldInfo = fields[i];
                    if(fieldInfo.GetCustomAttribute<HideInInspector>() != null)
                    {
                        continue;
                    }

                    Type type = fields[i].FieldType;
                    if (HasPropertyEditorOrSerializable(type))
                    {
                        propertyDescriptors.Add(new PropertyDescriptor(fieldInfo.Name, fieldInfo));
                    }
                }

                if (!memberInfoType.IsValueType)
                {
                    PropertyInfo[] properties = Reflection.GetSerializableProperties(memberInfoType);
                    for (int i = 0; i < properties.Length; ++i)
                    {
                        PropertyInfo propertyInfo = properties[i];
                        if (propertyInfo.GetCustomAttribute<HideInInspector>() != null)
                        {
                            continue;
                        }

                        Type type = properties[i].PropertyType;
                        if (HasPropertyEditorOrSerializable(type))
                        {
                            propertyDescriptors.Add(new PropertyDescriptor(propertyInfo.Name, propertyInfo));
                        }
                    }
                }
            }

            foreach (var descriptor in propertyDescriptors)
            {
                CreateElementEditor(descriptor);
            }
        }

        private void CreateElementEditor(PropertyDescriptor propertyDescriptor)
        {
            var type = propertyDescriptor.MemberType;
            var memberInfo = propertyDescriptor.MemberInfo;
            var label = propertyDescriptor.Label;

            if (memberInfo is MethodInfo)
            {
                type = typeof(MethodInfo);
            }

            if (!m_editorsMap.IsPropertyEditorEnabled(type))
            {
                return;
            }

            var editorPrefab = m_editorsMap.GetPropertyEditor(type);
            if (editorPrefab == null)
            {
                return;
            }
         
            var accessorsList = new List<CustomTypeFieldAccessor>();
            int targetsCount = Targets.Length;
            for (int i = 0; i < targetsCount; ++i)
            {
                accessorsList.Add(new CustomTypeFieldAccessor<object>(this, i, memberInfo, label));
            }

            if (accessorsList.Count > 0)
            {
                PropertyEditor editor = m_editorsMap.InstantiatePropertyEditor(propertyDescriptor, Panel);
                if (editor == null)
                {
                    return;
                }

                CustomTypeFieldAccessor[] accessors = accessorsList.ToArray();
                if (type == typeof(MethodInfo))
                {
                    editor.Init(accessors, Strong.MethodInfo((CustomTypeFieldAccessor<object> x) => x.Action()), accessors[0].Name, false, OnBeginRecordValue, OnEndRecordValue);
                }
                else
                {
                    editor.Init(accessors, Strong.MemberInfo((CustomTypeFieldAccessor<object> x) => x.Value), accessors[0].Name, false, OnBeginRecordValue, OnEndRecordValue);
                }
            }
        }

        private void OnBeginRecordValue()
        {
            BeginEdit();

            for (int i = 0; i < Targets.Length; ++i)
            {
                SetValue(Duplicate(GetValue(i)), i);
            }
        }

        private void OnEndRecordValue()
        {
            EndEdit();
        }

        private object Duplicate(object value)
        {
            if (value == null)
            {
                return null;
            }
            
            return JsonUtility.FromJson(JsonUtility.ToJson(value), value.GetType());
        }

        protected override void SetInputField(object value)
        {
            if (value == null)
            {
                if(MemberInfoType.IsArray)
                {
                    Array newArray = (Array)Activator.CreateInstance(MemberInfoType, 0);
                    SetValue(newArray);
                    return;
                }
            }
        }

        protected override void ReloadOverride(bool force, bool raiseValueChanged)
        {
            if(force)
            {
                object value = GetValue();
                DoReload(value);
                RaiseReloadCallback();
            }
            else
            {
                object value = GetValue();
                if (!EqualityComparer<object>.Default.Equals(CurrentValue, value))
                {
                    Type memberInfoType = value != null ? value.GetType() : MemberInfoType;
                    if (!Reflection.IsValueType(memberInfoType) || !Equals(memberInfoType, CurrentValue, value))
                    {
                        DoReload(value);

                        if (raiseValueChanged)
                        {
                            RaiseValueChanged();
                        }

                        RaiseReloadCallback();
                    }
                }
            }
        }

        private void DoReload(object value)
        {
            CurrentValue = value;
            SetInputField(value);
            BuildEditor();
        }

        private bool Equals(Type memberInfoType, object currentValue, object value)
        {
            FieldInfo[] fields = Reflection.GetSerializableFields(memberInfoType, false);
            PropertyInfo[] properties = Reflection.GetSerializableProperties(memberInfoType);
            
            for (int i = 0; i < fields.Length; ++i)
            {
                FieldInfo fieldInfo = fields[i];
                if (!m_editorsMap.IsPropertyEditorEnabled(fieldInfo.FieldType))
                {
                    continue;
                }

                if(ChildDescriptors != null && !ChildDescriptors.ContainsKey(fieldInfo))
                {
                    continue;
                }

                object c = fieldInfo.GetValue(currentValue);
                object v = fieldInfo.GetValue(value);
                if (c == null && v == null)
                {
                    continue;
                }
                if (c == null || v == null || !c.Equals(v))
                {
                    return false;
                }
            }

            for (int i = 0; i < properties.Length; ++i)
            {
                PropertyInfo propertyInfo = properties[i];
                if (!m_editorsMap.IsPropertyEditorEnabled(propertyInfo.PropertyType))
                {
                    continue;
                }

                if (ChildDescriptors != null && !ChildDescriptors.ContainsKey(propertyInfo))
                {
                    continue;
                }

                object c = propertyInfo.GetValue(currentValue);
                object v = propertyInfo.GetValue(value);
                if (c == null && v == null)
                {
                    continue;
                }
                if (c == null || v == null || !c.Equals(v))
                {
                    return false;
                }
            }

            return true;
        }


        private IEnumerator m_coExpand;
        private IEnumerator CoExpand()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            Expander.isOn = true;
            m_coExpand = null;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (Editor.DragDrop.InProgress)
            {
                if (Expander != null)
                {
                    m_coExpand = CoExpand();
                    StartCoroutine(m_coExpand);
                }
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (m_coExpand != null)
            {
                StopCoroutine(m_coExpand);
                m_coExpand = null;
            }
        }
    }

    public abstract class CustomTypeFieldAccessor
    {
        public Type Type
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public CustomTypeFieldAccessor(string name, MemberInfo memberInfo)
        {
            Name = name;
            if (memberInfo is PropertyInfo)
            {
                PropertyInfo pInfo = (PropertyInfo)memberInfo;
                Type = pInfo.PropertyType;
            }
            else if (memberInfo is FieldInfo)
            {
                FieldInfo fInfo = (FieldInfo)memberInfo;
                Type = fInfo.FieldType;
            }
            else if (memberInfo is MethodInfo)
            {
                Type = typeof(MethodInfo);  
            }
        }
    }

    public class CustomTypeFieldAccessor<T> : CustomTypeFieldAccessor
    {
        private int m_index;

        private MemberInfo m_memberInfo;
        private PropertyEditor<T> m_editor;

        public object Value
        {
            get
            {
                object obj = m_editor.GetValue(m_index);
                if (obj == null)
                {
                    return null;
                }

                if (m_memberInfo is FieldInfo)
                {
                    return ((FieldInfo)m_memberInfo).GetValue(obj);
                }

                if (m_memberInfo is PropertyInfo)
                {
                    return ((PropertyInfo)m_memberInfo).GetValue(obj, null);
                }

                return default(T);
            }
            set
            {
                int targetsCount = m_editor.Target != null ? m_editor.Targets.Length : 0;
                //m_editor.BeginEdit();
                for (int i = 0; i < targetsCount; ++i)
                {
                    T obj = m_editor.GetValue(i);
                    if (m_memberInfo is FieldInfo)
                    {
                        ((FieldInfo)m_memberInfo).SetValue(obj, value);
                    }
                    else if (m_memberInfo is PropertyInfo)
                    {
                        ((PropertyInfo)m_memberInfo).SetValue(obj, value, null);
                    }
                    m_editor.SetValue(obj, i);
                }
                //m_editor.EndEdit();
            }
        }

        public void Action()
        {
            var mi = m_memberInfo as MethodInfo;
            if (mi != null)
            {
                int targetsCount = m_editor.Target != null ? m_editor.Targets.Length : 0;
                for (int i = 0; i < targetsCount; ++i)
                {
                    var target = m_editor.GetValue(i);
                    mi.Invoke(target, new object[0]);
                }
            }
            m_editor.EndEdit();
        }

        public CustomTypeFieldAccessor(PropertyEditor<T> editor, int index, MemberInfo memberInfo, string name) : base(name, memberInfo)
        {
            m_index = index;
            m_editor = editor;
            m_memberInfo = memberInfo;
        }
    }
}

