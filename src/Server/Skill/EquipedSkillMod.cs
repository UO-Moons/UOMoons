namespace Server;

public class EquipedSkillMod : SkillMod
{
	private readonly Item _item;
	private readonly Mobile _mobile;

	public EquipedSkillMod(SkillName skill, bool relative, double value, Item item, Mobile mobile)
		: base(skill, relative, value)
	{
		_item = item;
		_mobile = mobile;
	}

	public override bool CheckCondition()
	{
		return !_item.Deleted && !_mobile.Deleted && _item.Parent == _mobile;
	}
}
