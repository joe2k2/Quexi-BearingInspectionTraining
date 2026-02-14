using UnityEngine;

public class QuizInteractionPart : MonoBehaviour
{
    public int partIndex;
    public PartInfoUIManager manager;

    public void OnSnapped()
    {
        if (manager != null)
        {
            manager.OnInteractionPartSnapped(partIndex);
        }
    }
}
