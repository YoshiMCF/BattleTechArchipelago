using System;

namespace BattleTechArchipelago;

[AttributeUsage(AttributeTargets.Method)]
public class ItemHandlerAttribute(long id) : Attribute {
	public long id = id;
}
