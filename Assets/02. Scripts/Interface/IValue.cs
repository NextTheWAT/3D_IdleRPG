/// <summary>
/// 외부에서 값 증감 기능을 제공하기 위한 인터페이스.
/// int 단위로 값을 증감시키고, 변경된 최종 float 값을 반환한다.
/// </summary>
public interface IValueChangable
{
    float ValueChanged(int amount);
}
