[System.Serializable]
public class CardData
{

    public bool Used;               // ���� ����� �������������� ������, �� ����� ���������� ������ ������� ��� ���������� (���� � ������ ����� 2 �����)
    // ������������ ���������
    public string Guid;             // ���������� �������������
    public string Version;          // ������ (����������� ������)

    // ����������� ���������
    public string Name;             // �������� �����
    public string Type;            // ����������� �������
    public int EnergyDamage;        // ���� �� �������
    public int CapitalDamage;       // ���� �� ��������
    public int EnergyHealth;        // ������� �������
    public int CapitalEarnings;     // ���������
    public int DamageResistance;    // ������ �� ����� �� ��������
    public int CardCost;            // ����������� �������
    public string ChipId;
}
