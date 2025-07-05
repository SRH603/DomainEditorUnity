using UnityEngine;

namespace Battlehub.Storage.Enumerators.UnityEngine
{
    [ObjectEnumerator(typeof(Material))]
    public class MaterialEnumerator : ObjectEnumerator<Material>
    {
        private static readonly IShaderUtil m_shaderUtil = new RTShaderUtil();
        private RTShaderInfo m_shaderInfo;
        
        public override object Object
        {
            get { return base.Object; }
            set 
            {
                base.Object = value;

                Material material = TypedObject;
                if(material != null)
                {
                    m_shaderInfo = m_shaderUtil.GetShaderInfo(material.shader);
                }
                else
                {
                    m_shaderInfo = null;
                }
            }
        }

        public override bool MoveNext()
        {
            do
            {
                int propertyCount = 0;
                if (m_shaderInfo != null)
                {
                    propertyCount = m_shaderInfo.PropertyCount;
                }

                if(Index < propertyCount)
                {
                    if(m_shaderInfo.PropertyTypes[Index] == RTShaderPropertyType.TexEnv)
                    {
                        string propertyName = m_shaderInfo.PropertyNames[Index];
                        int propertyKey = propertyName.GetHashCode(); // HACK
                        if (MoveNext(TypedObject.GetTexture(propertyName), propertyKey))
                            return true;
                    }
                    else
                    {
                        Index++;
                    }
                }
                else if(Index == propertyCount)
                {
                    if (MoveNext(Object, -1))
                        return true;
                }
                else
                {
                    return false;
                }
            }
            while (true);
        }
    }
}