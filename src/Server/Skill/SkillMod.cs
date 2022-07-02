namespace Server;

public abstract class SkillMod
{
	private Mobile _owner;
	private SkillName _skill;
	private bool _relative;
	private double _value;
	private bool _obeyCap;

	protected SkillMod(SkillName skill, bool relative, double value)
	{
		_skill = skill;
		_relative = relative;
		_value = value;
	}

	public bool ObeyCap
	{
		get => _obeyCap;
		set
		{
			_obeyCap = value;

			if (_owner != null)
			{
				Skill sk = _owner.Skills[_skill];

				sk?.Update();
			}
		}
	}

	public Mobile Owner
	{
		get => _owner;
		set
		{
			if (_owner != value)
			{
				_owner?.RemoveSkillMod(this);

				_owner = value;

				if (_owner != value)
					_owner.AddSkillMod(this);
			}
		}
	}

	public void Remove()
	{
		Owner = null;
	}

	public SkillName Skill
	{
		get => _skill;
		set
		{
			if (_skill != value)
			{
				Skill oldUpdate = _owner?.Skills[_skill];

				_skill = value;

				Skill sk = _owner?.Skills[_skill];

				sk?.Update();

				oldUpdate?.Update();
			}
		}
	}

	public bool Relative
	{
		get => _relative;
		set
		{
			if (_relative != value)
			{
				_relative = value;

				Skill sk = _owner?.Skills[_skill];

				sk?.Update();
			}
		}
	}

	public bool Absolute
	{
		get => !_relative;
		set
		{
			if (_relative == value)
			{
				_relative = !value;

				Skill sk = _owner?.Skills[_skill];

				sk?.Update();
			}
		}
	}

	public double Value
	{
		get => _value;
		set
		{
			if (_value != value)
			{
				_value = value;

				Skill sk = _owner?.Skills[_skill];

				sk?.Update();
			}
		}
	}

	public abstract bool CheckCondition();
}
