using System;
namespace DGSave
{
	//!  LSMethod 
	/*!
This enum encompasses all supported Save/Load methods, keep in mind that Resource is READONLY.
	*/
	public enum LSMethod{
		/// <summary>
		/// PlayerPrefs
		/// </summary>
		Prefs,
		/// <summary>
		/// File
		/// </summary>
		File,
		/// <summary>
		/// Web
		/// </summary>
		Web,
		/// <summary>
		/// Resource
		/// </summary>
		Resource
	}
	
	//!  LSTypes 
	/*!
This enum encompasses all supported Object types
*/
	public enum LSTypes{
		/// <summary>
		/// Int
		/// </summary>
		Int=1,
		/// <summary>
		/// Bool
		/// </summary>
		Bool=2,
		/// <summary>
		/// Byte
		/// </summary>
		Byte=3,
		/// <summary>
		/// Char
		/// </summary>
		Char=4,
		/// <summary>
		/// Double
		/// </summary>
		Double=5,
		/// <summary>
		/// Float
		/// </summary>
		Float=6,
		/// <summary>
		/// Short
		/// </summary>
		Short=7,
		/// <summary>
		/// Long
		/// </summary>
		Long=8,
		/// <summary>
		/// Object
		/// </summary>
		Object=9,
		/// <summary>
		/// String
		/// </summary>
		String=10,
		/// <summary>
		/// UShort
		/// </summary>
		UShort=11,
		/// <summary>
		/// UInt
		/// </summary>
		UInt=12,
		/// <summary>
		/// ULong
		/// </summary>
		ULong=13,
		/// <summary>
		/// Transform
		/// </summary>
		Vector4=14,
		/// <summary>
		/// Vector3
		/// </summary>
		Vector3 = 15,
		/// <summary>
		/// Vector2
		/// </summary>
		Vector2 = 16,
		/// <summary>
		/// Quaternion
		/// </summary>
		Quaternion = 17,
		/// <summary>
		/// Rect
		/// </summary>
		Rect=18
		
	}
}