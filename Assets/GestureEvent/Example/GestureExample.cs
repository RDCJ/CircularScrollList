using GestureEvent;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GestureExample : MonoBehaviour
{
    // Start is called before the first frame update
    private Color[] colors = new Color[2] { Color.white, Color.black };
    private int idx = 0;
    public Image img;
    private RectTransform img_rtf;
    private BoxCollider2D img_boxCollider;
    private Rigidbody2D img_rb;

    private int DragFingerId = -1;

    private void Awake()
    {
        img_rtf = img.GetComponent<RectTransform>();
        img_boxCollider = img.GetComponent<BoxCollider2D>();
        img_rb = img.GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GestureMgr.Inst.AddListener<ClickGestureRecognizer>(onClick);
        GestureMgr.Inst.AddListener<TapMoveRecognizer>(onTapMove);
        GestureMgr.Inst.AddListener<HoldGestureRecognizer>(onHold);
        GestureMgr.Inst.AddListener<ScaleGestureRecognizer>(onScale);
        GestureMgr.Inst.AddListener<MoveGestureRecognizer>(onMove);
        GestureMgr.Inst.AddListener<EndGestureRecognizer>(onEnd);
        GestureMgr.Inst.AddListener<StationaryGestureRecognizer>(onStationary);
    }

    private bool IsHit(Vector2 screen_pos)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(img_rtf, screen_pos, Camera.main);
    }

    private void onClick(IGestureMessage message)
    {
        var clickMessage = message as ClickMessage;
        if (IsHit(clickMessage.press_position) && IsHit(clickMessage.release_position))
        {
            idx++;
            img.color = colors[idx % 2];
        }
    }

    private void onTapMove(IGestureMessage message)
    {
        var tapMoveMessage = message as TapMoveMessage;
        if (IsHit(tapMoveMessage.press_position))
        {
            img_rb.AddForce(tapMoveMessage.delta_position.normalized * 500);
        }
    }

    private void onHold(IGestureMessage message)
    {
        var holdMessage = message as HoldMessage;
        if (DragFingerId != -1) return;
        if (IsHit(holdMessage.current_position))
        {
            DragFingerId = holdMessage.fingerId;
            ScaleAnimation(1.1f, 0.15f);
        }
    }

    private void onStationary(IGestureMessage message)
    {
        var stationaryMessage = message as StationaryMessage;
        if (stationaryMessage.fingerId == DragFingerId)
        {
            Vector3 world_pos = Camera.main.ScreenToWorldPoint(stationaryMessage.position);
            img_rtf.position = world_pos;
            img_rtf.localPosition = new Vector3(img_rtf.localPosition.x, img_rtf.localPosition.y, 0);
            img_rb.velocity = Vector2.zero;
        }
    }

    private void onMove(IGestureMessage message)
    {
        var moveMessage = message as MoveMessage;
        if (moveMessage.fingerId == DragFingerId)
        {
            Vector3 world_pos = Camera.main.ScreenToWorldPoint(moveMessage.position);
            img_rtf.position = world_pos;
            img_rtf.localPosition = new Vector3(img_rtf.localPosition.x, img_rtf.localPosition.y, 0);
            img_rb.velocity = Vector2.zero;
        }
    }

    private void onEnd(IGestureMessage message)
    {
        var endMessage = message as EndMessage;
        if (endMessage.fingerId == DragFingerId) 
        {
            ScaleAnimation(1f, 0.15f);
            DragFingerId = -1;
        }
            
    }

    private void onScale(IGestureMessage message)
    {
        var scaleMessage = message as ScaleMessage;
        float deltaScale = (scaleMessage.currentDistance - scaleMessage.previousDistance) * 0.01f;
        img_rtf.localScale += new Vector3(deltaScale, deltaScale, deltaScale);
    }

    private void ScaleAnimation(float target_scale, float scale_time)
    {
        float start_scale = img_rtf.localScale.x;
        float delta_scale = (target_scale - start_scale) / scale_time;
        Vector3 delta_scale_v = new Vector3(delta_scale, delta_scale, delta_scale);
        IEnumerator _ScaleAnimation()
        {
            while (true)
            {
                if (target_scale > start_scale)
                    if (img_rtf.localScale.x >= target_scale) break;
                if (target_scale < start_scale)
                    if (img_rtf.localScale.x <= target_scale) break;
                img_rtf.localScale += delta_scale_v * Time.deltaTime;
                yield return null;
            }
        }
        StartCoroutine(_ScaleAnimation());
    }
}
