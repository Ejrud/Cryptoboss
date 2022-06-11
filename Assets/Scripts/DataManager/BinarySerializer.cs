using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class BinarySerializer
{
    public static void Serialize(string path, object data)
    {
        using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, data);
        }
    }

    public static T Deserialize<T>(string path)
    {
        using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            T data = (T) formatter.Deserialize(stream);
            return data;
        }
    }
}
