using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json; // version volontairement ancienne dans le csproj

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Démo des mauvaises pratiques (éducatif) ===\n");

        // 1) Informations sensibles codées en dur
        var dbUser = "admin";                     // Mauvaise pratique: identifiants en clair
        var dbPassword = "P@ssw0rd!";             // -> risque si repo fuit

        Console.WriteLine("1) Credentials codés en dur : " + dbUser);

        // 2) Construction de requête SQL par concaténation (risque d'injection)
        var usernameInput = "alice"; // dans la vraie vie, ceci vient d'un utilisateur
        string insecureSql = "SELECT * FROM Users WHERE username = '" + usernameInput + "';";
        Console.WriteLine("2) Exemple SQL construit par concaténation: ");
        Console.WriteLine(insecureSql);

        // 3) Désactivation de la validation TLS (accept all certs) — dangereux
        MakeInsecureHttpRequest("https://example.com/");

        // 4) Utilisation d'une dépendance ancienne (Newtonsoft.Json v10) — risque de vulnérabilités connues
        var obj = new { Name = "Alice", Age = 30 };
        string json = JsonConvert.SerializeObject(obj); // OK mais la version du package est ancienne
        Console.WriteLine("4) JSON (via dépendance ancienne) : " + json);

        // 5) Oublier de disposer un flux -> fuite de ressources
        FileStream fs = null!;
        try
        {
            fs = new FileStream("temp.txt", FileMode.Create);
            var bytes = Encoding.UTF8.GetBytes("some text");
            fs.Write(bytes, 0, bytes.Length);
            // Oops: pas de using ni de fs.Dispose() dans le finally
        }
        catch (Exception ex)
        {
            // Mauvaise pratique: catch trop générique, cache l'erreur réelle
            Console.WriteLine("Erreur: " + ex.Message);
        }

        Console.WriteLine("\n=== Fin de la démo. Voir les notes pour corrections. ===");
    }

    // Méthode démontrant la désactivation de la validation TLS (NE PAS UTILISER EN PROD)
    static void MakeInsecureHttpRequest(string url)
    {
        Console.WriteLine("\n-> Envoi d'une requête HTTP avec la validation TLS désactivée (mauvaise pratique)");
        // Handler qui accepte tous les certificats (dangereux)
        var handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        using var client = new HttpClient(handler);
        try
        {
            var resp = client.GetAsync(url).Result; // blocage synchrone (mauvaise pratique)
            Console.WriteLine($"Statut: {resp.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Req erreur: " + ex.Message);
        }
    }
}
