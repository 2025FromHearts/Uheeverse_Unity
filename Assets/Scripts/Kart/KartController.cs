using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;

public class KartController : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();
    }
    [Header("Wheel Colliders")]
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider backLeft;
    public WheelCollider backRight;

    [Header("Wheel Meshes")]
    public Transform meshFL;
    public Transform meshFR;
    public Transform meshBL;
    public Transform meshBR;

    [Header("Body")]
    public Transform body;
    private Vector3 bodyInitialLocalPos;
    private Quaternion bodyInitialLocalRot;

    [Header("Settings")]
    public float maxMotorTorque = 3000f;    // 기본 가속 토크
    public float maxSteerAngle = 10f;       // 조향 각도
    public float turnSpeed = 1f;            // 회전 민감도

    private Rigidbody rb;

    // 아이템 효과 관련
    private bool controlsReversed = false;  // 조작 반전 여부
    private bool isSpinning = false;        // 제자리 회전 중인지 여부
    private float speedMultiplier = 1f;     // 현재 속도 배율 (1 = 기본 속도)

    // 외부에서 효과를 적용할 수 있도록 메서드 제공
    public void SetSpeedMultiplier(float multiplier) => speedMultiplier = multiplier;
    public void SetReverseControl(bool reverse) => controlsReversed = reverse;
    public void SetSpinning(bool spin) => isSpinning = spin;

    //바나나
    public GameObject bananaPrefab;  // 바나나 프리팹
    public Transform bananaPivot;    // 카트 중심에서 회전할 기준점
    private List<GameObject> orbitingBananas = new List<GameObject>();
    //private int bananaCount = 0;

    //아이템 이미지UI 연결
    public ItemDisplayUI itemDisplayUI;
    void Update()
    {
        if(!base.IsOwner)  { return; }
        // BananaPivot 회전
        if (orbitingBananas.Count > 0)
        {
            bananaPivot.Rotate(Vector3.up * 100f * Time.deltaTime);
        }

        // 바나나 발사
        if (Input.GetKeyDown(KeyCode.K) && orbitingBananas.Count > 0)
        {
            GameObject banana = orbitingBananas[0];
            orbitingBananas.RemoveAt(0);

            banana.transform.parent = null;

            Banana bananaScript = banana.GetComponent<Banana>();
            bananaScript.isOrbiting = false;

            Rigidbody rb = banana.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.linearVelocity = transform.forward * 15f;

            Destroy(banana, 5f);
        }
    }

    //바나나 회전
    void RotateBananasAroundKart()
    {
        float radius = 1.5f;
        float speed = 100f;

        for (int i = 0; i < orbitingBananas.Count; i++)
        {
            float angle = Time.time * speed + (360f / orbitingBananas.Count) * i;
            Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
            orbitingBananas[i].transform.position = bananaPivot.position + offset;
        }
    }

    void AdjustWheelFriction(WheelCollider wheel)
    {
        WheelFrictionCurve forwardFriction = wheel.forwardFriction;
        forwardFriction.extremumValue = 3f;
        forwardFriction.asymptoteValue = 2f;
        forwardFriction.stiffness = 1.5f; // 기본 1보다 크게

        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
        sidewaysFriction.extremumValue = 2f;
        sidewaysFriction.asymptoteValue = 1.5f;
        sidewaysFriction.stiffness = 0.8f;

        wheel.forwardFriction = forwardFriction;
        wheel.sidewaysFriction = sidewaysFriction;
    }

    //바나나
    // KartController.cs 안의 AddOrbitingBananas 함수
    void AddOrbitingBananas(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject b = Instantiate(bananaPrefab, bananaPivot);
            b.transform.localPosition = Vector3.zero;
            b.GetComponent<Rigidbody>().isKinematic = true;

            // Destroy 안 하도록 Banana.cs 에서 꺼주는 방식도 가능
            Banana bananaScript = b.GetComponent<Banana>();
            bananaScript.isOrbiting = true; // orbit 상태임을 표시

            orbitingBananas.Add(b);
        }
    }





    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.8f, 0); // 무게중심을 아래로 이동 (안정성 ↑)

        rb.linearDamping = 0.02f; // 공기저항 최소화
        rb.angularDamping = 0.05f;
        // 휠 마찰 조정
        AdjustWheelFriction(frontLeft);
        AdjustWheelFriction(frontRight);
        AdjustWheelFriction(backLeft);
        AdjustWheelFriction(backRight);

        if (body != null)
        {
            bodyInitialLocalPos = body.localPosition;
            bodyInitialLocalRot = body.localRotation;
        }
        //speedMultiplier = 1f;
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.magnitude > 50f)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * 50f;
        }
        if (isSpinning) return; // 회전 중에는 조작 불가

        // 이동 입력 (뒤로 향해 있는 모델 대응을 위해 - 부호)
        float slopeAngle = Vector3.Angle(transform.up, Vector3.up);
        float slopeTorqueBoost = Mathf.Clamp(slopeAngle * 100f, 0, 10000f);

        float motor = (-Input.GetAxis("Vertical") * (maxMotorTorque + slopeTorqueBoost)) * speedMultiplier;

        // 좌우 조향 입력 (조작 반전 고려)
        float steerInput = Input.GetAxis("Horizontal") * (controlsReversed ? -1f : 1f);
        float steerAngle = steerInput * maxSteerAngle;

        // 조향 적용 (전/후륜 모두)
        frontLeft.steerAngle = steerAngle*2f;
        frontRight.steerAngle = steerAngle*2f;
        backLeft.steerAngle = steerAngle * 2f;
        backRight.steerAngle = steerAngle * 2f;

        // 구동력 적용 (후륜 구동)
        //backLeft.motorTorque = motor;
        //backRight.motorTorque = motor;

        // 모든 바퀴에 구동력 분배
        frontLeft.motorTorque = motor * 0.4f;
        frontRight.motorTorque = motor * 0.4f;
        backLeft.motorTorque = motor * 0.6f;
        backRight.motorTorque = motor * 0.6f;

        // 브레이크 제거
        backLeft.brakeTorque = 0f;
        backRight.brakeTorque = 0f;

        // 몸체 회전 보정
        if (Mathf.Abs(steerInput) > 0.1f)
        {
            float turnAmount = steerInput * turnSpeed;
            float adjustedTurn = Mathf.Clamp(rb.linearVelocity.magnitude, 0.5f, 5f) * turnAmount;
            Quaternion turnOffset = Quaternion.Euler(0f, adjustedTurn, 0f);
            rb.MoveRotation(rb.rotation * turnOffset);
        }

        Debug.Log($"[Kart] motor: {motor}, speedMultiplier: {speedMultiplier}, velocity: {rb.linearVelocity.magnitude}");

        // 바퀴 메쉬 회전 적용
        UpdateWheelPose(frontLeft, meshFL, "FL");
        UpdateWheelPose(frontRight, meshFR, "FR");
        UpdateWheelPose(backLeft, meshBL, "BL");
        UpdateWheelPose(backRight, meshBR, "BR");

        // 본체 위치 보정
        if (body != null)
        {
            body.position = transform.TransformPoint(bodyInitialLocalPos);
            body.rotation = transform.rotation * bodyInitialLocalRot;
        }

    }

    //바퀴랑 본체 사이 멀어짐
    void UpdateWheelPose(WheelCollider collider, Transform mesh, string wheelName)
    {
        Vector3 pos;
        Quaternion quat;
        collider.GetWorldPose(out pos, out quat);
        mesh.position = pos;

        Quaternion rotationFix = Quaternion.identity;
        switch (wheelName)
        {
            case "FL":
            case "BL":
                rotationFix = Quaternion.Euler(0, 180, 90);
                break;
            case "FR":
            case "BR":
                rotationFix = Quaternion.Euler(0, 0, 90);
                break;
        }

        mesh.rotation = quat * rotationFix;
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ItemBox"))
        {
            Debug.Log("아이템 박스 충돌!");

            ItemBox box = other.GetComponent<ItemBox>();
            if (box != null)
            {
                Debug.Log("적용된 효과: " + box.effectType);
                ApplyEffect(box.effectType);
            }

            other.gameObject.SetActive(false);
        }
    }


    // 아이템 효과 적용
    public void ApplyEffect(ItemEffectType effect)
    {
        itemDisplayUI.SetItemIcon(effect); // 아이템 효과 연결

        if (itemDisplayUI != null)
        {
            itemDisplayUI.ShowItem(effect);
        }

        switch (effect)
        {
            case ItemEffectType.SpeedUp:
                StartCoroutine(SpeedBoost(30f, 3f));  // 기본보다 빠르게
                break;
            case ItemEffectType.SlowDown:
                StartCoroutine(SpeedBoost(0f, 3f)); // 절반 속도, (속도값,초)
                break;
            case ItemEffectType.Spin:
                StartCoroutine(SpinInPlace());
                break;
            case ItemEffectType.ReverseControl:
                StartCoroutine(ReverseControls(5f));
                break;
            case ItemEffectType.Banana:
                AddOrbitingBananas(3); // 바나나
                break;
        }
    }

    IEnumerator SpeedBoost(float multiplier, float duration)
    {
        speedMultiplier = multiplier;
        Debug.Log($"[SpeedBoost] {multiplier}x 속도 적용");
        yield return new WaitForSeconds(duration);
        speedMultiplier = 1f;
        Debug.Log("[SpeedBoost] 속도 복구");
    }

    // 조작 반전
    IEnumerator ReverseControls(float duration)
    {
        controlsReversed = true;
        Debug.Log("[Reverse] 조작 반전 시작");
        yield return new WaitForSeconds(duration);
        controlsReversed = false;
        Debug.Log("[Reverse] 조작 복귀");
    }

    // 제자리 회전
    IEnumerator SpinInPlace()
    {
        isSpinning = true;
        float spinDuration = 1.5f;
        float spinSpeed = 720f;
        float time = 0f;

        while (time < spinDuration)
        {
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
            time += Time.deltaTime;
            yield return null;
        }

        isSpinning = false;
    }

    //마지막에 카트 멈추기
    public void StopKartGradually()
    {
        StartCoroutine(GradualStop());
    }

    private IEnumerator GradualStop()
    {
        float t = 0f;
        float duration = 2f;
        Vector3 initialVelocity = rb.linearVelocity;

        while (t < duration)
        {
            rb.linearVelocity = Vector3.Lerp(initialVelocity, Vector3.zero, t / duration);
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        enabled = false; // 입력 비활성화
    }

}
