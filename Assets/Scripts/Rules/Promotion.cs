using System.Text;
using Newtonsoft.Json;

[System.Serializable]
public struct Promotion
{
    public Team team;
    public Piece from;
    public Piece to; 
    public Promotion(Team team, Piece from, Piece to)
    {
        this.team = team;
        this.from = from;
        this.to = to;
    }

    public byte[] Serialize() => Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this));
    public static Promotion Deserialize(byte[] data) => JsonConvert.DeserializeObject<Promotion>(Encoding.ASCII.GetString(data));
}