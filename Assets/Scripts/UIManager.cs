using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("상단 체력 UI (왼쪽 위)")]
    public Slider healthSlider;
    public Image healthFillImage; // 빨간색 채우기용 이미지

    [Header("하단 스킬 슬롯 UI (가운데 아래)")]
    public Image[] skillSlots; // 1, 2, 3번 스킬 아이콘 담을 슬롯들

    [Header("가방 UI")]
    public GameObject inventoryPanel; // 가방 버튼 누르면 켜질 UI 창 패널

    void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false); // 시작 시 인벤토리는 꺼둠
        }
    }

    // 체력 바 업데이트
    public void SetHealth(float current, float max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
    }

    // 스킬 사용 시 UI 반응 (예: 아주 단순한 쿨타임 애니메이션 가동용 등)
    public void OnSkillUsed(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < skillSlots.Length)
        {
            Debug.Log($"UI: {slotIndex + 1}번 스킬 슬롯 활성화 이펙트");
            // 이곳에 쿨타임 스크립트를 붙이거나 연출을 추가할 수 있습니다.
        }
    }

    // 가방(인벤토리) 버튼을 클릭했을 때 실행할 함수
    public void OnBagButtonClicked()
    {
        if (inventoryPanel != null)
        {
            // 인벤토리 창 켜져있으면 끄고, 꺼져있으면 켜기 (토글)
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
            Debug.Log($"가방 상태 변경: {inventoryPanel.activeSelf}");
        }
    }
}