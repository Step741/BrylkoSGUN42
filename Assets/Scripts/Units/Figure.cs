using Utils;

public class Figure
{
    public FigureType Type
    {
        get
        {
            return _type;
        }

        set
        {
            if (value != FigureType.None)
            {
                _type = value;
            }
        }
    }


    public readonly Team Team;
    public readonly string Sign;

    private FigureType _type;

    public Figure(FigureType type, Team team)
    {
        Type = type;
        Team = team;
    }

    public Figure(FigureType type, Team team, string s)
    {
        Type = type;
        Team = team;
        Sign = s;
    }
}
