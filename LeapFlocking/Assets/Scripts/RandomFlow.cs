using System.Collections;
using UnityEngine;

public class RandomFlow : MonoBehaviour
{
    public float speed = 1f;           // パーリンノイズの動きの速さ
    public float amplitude = 1f;      // パーリンノイズの振幅
    public float avoidSpeed = 2f;     // 回避行動の速さ
    public float avoidDistance = 2f;  // 回避を開始する距離
    public float avoidDuration = 3f;  // 回避行動を続ける時間
    public float returnSpeed = 1f;    // 元の位置に戻る速さ

    private Vector3 initialPosition;  // 初期位置
    private GameObject leapmotion;    // LeapMotionのオブジェクト
    private HandDataGetter user;      // ユーザー情報を取得するクラスの参照
    private bool isAvoiding = false;  // 回避行動中かどうか
    private bool isReturning = false; // 元の位置に戻っている途中かどうか

    void Start()
    {
        // 初期設定
        initialPosition = transform.position;
        leapmotion = GameObject.Find("LeapMotionManager");
        user = leapmotion.GetComponent<HandDataGetter>();
    }

    void Update()
    {
        if (!isAvoiding && !isReturning && user.UserPos.HasValue)
        {
            Vector3 userPos = user.UserPos.Value;

            // ユーザーとの距離を計算
            float distance = Vector3.Distance(transform.position, userPos);

            if (distance < avoidDistance)
            {
                // 回避行動を開始
                StartCoroutine(AvoidRoutine(userPos));
            }
        }

        if (!isAvoiding && !isReturning)
        {
            // 通常の動き
            UpdatePos();
        }
    }

    void UpdatePos()
    {
        // パーリンノイズを使用した動き
        float offsetX = Mathf.PerlinNoise(Time.time * speed, 0f) * 2f - 1f; // -1から1の範囲
        float offsetY = Mathf.PerlinNoise(0f, Time.time * speed) * 2f - 1f; // -1から1の範囲
        float offsetZ = Mathf.PerlinNoise(Time.time * speed, Time.time * speed) * 2f - 1f; // -1から1の範囲

        Vector3 offset = new Vector3(offsetX, offsetY, offsetZ) * amplitude;
        transform.position = initialPosition + offset;
    }

    IEnumerator AvoidRoutine(Vector3 userPos)
    {
        isAvoiding = true;
        float elapsedTime = 0f;

        while (elapsedTime < avoidDuration)
        {
            // ユーザーから遠ざかる方向を計算
            Vector3 direction = (transform.position - userPos).normalized;

            // 遠ざかる速度を適用
            transform.position += direction * avoidSpeed * Time.deltaTime;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 回避行動終了後に元の位置に戻る
        StartCoroutine(ReturnToInitialPosition());
        isAvoiding = false;
    }

    IEnumerator ReturnToInitialPosition()
    {
        isReturning = true;

        while (Vector3.Distance(transform.position, initialPosition) > 0.1f)
        {
            // 元の位置に近づく
            transform.position = Vector3.Lerp(transform.position, initialPosition, returnSpeed * Time.deltaTime);
            yield return null;
        }

        // 正確に元の位置に戻る
        transform.position = initialPosition;
        isReturning = false;
    }
}