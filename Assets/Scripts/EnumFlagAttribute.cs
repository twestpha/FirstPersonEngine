using UnityEngine;

//##################################################################################################
// Enum Flag Attribute
// Editor metadata for drawing a enum with distinct flag values in the unity editor
//##################################################################################################
public class EnumFlagAttribute : PropertyAttribute
{
	public string enumName;

	public EnumFlagAttribute() {}

	public EnumFlagAttribute(string name)
	{
		enumName = name;
	}
}
