using System.Collections;

public interface ISkillEffect
{
    IEnumerator Apply(SkillCastContext context);
}
