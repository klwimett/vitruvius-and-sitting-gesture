using UnityEngine;

public class Instrument : MonoBehaviour
{
    Collider2D boundsCollider;
    AudioSource audioSource;

    bool hit = false;
    Transform prevHand = null;

    void Awake()
    {
        boundsCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    public void Validate(Transform hand)
    {
        // Avoid double validation from another hand when one
        // is still registered
        if (prevHand != null && prevHand != hand &&
            hit && boundsCollider.OverlapPoint(prevHand.position))
        {
            return;
        }

        bool overlap = boundsCollider.OverlapPoint(hand.position);

        if (overlap && !hit)
        {
            audioSource.Play();
            hit = true;
        }

        if (!overlap && hit)
        {
            hit = false;
        }

        prevHand = hand;
    }
}