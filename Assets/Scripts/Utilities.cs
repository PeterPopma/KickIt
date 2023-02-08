using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class Utilities
{
    public static void LogToFile(string filename, string value)
    {
        using StreamWriter file = new(filename, append: true);
        file.WriteLineAsync(value);
    }

}
