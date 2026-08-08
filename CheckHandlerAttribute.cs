using System;

namespace BattleTechArchipelago;

[AttributeUsage(AttributeTargets.Method)]
public class CheckHandlerAttribute(long id) : Attribute {
	public long id = id;
}