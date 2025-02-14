using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New CharacterDatabase", menuName = "Characters/Database")]
public class CharacterDatabase : ScriptableObject
{
    [SerializeField] private Character[] characters = new Character[0];

    //a get method to retun all characters
    public Character[] GetAllCharacters() => characters;

    public Character GetCharacterById(int id)
    {
        foreach (Character character in characters)
        {
            if (character.Id == id) { return character; }
        }
        return null;
    }
    public bool IsValidCharacterId(int id)
    {
        //go thru each char (x) in the list, is each characters Id is in the same as the one we are trying to check. To see if the passed character Id is valid
        return characters.Any(x => x.Id == id);
    }
}
