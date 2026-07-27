using Seed.Models;

namespace Seed.Services;

public static class SeedContent
{
    public static readonly GrowthStage[] Stages =
    [
        new(1, 0, "씨앗", "시작은 보이지 않는 곳에서 먼저 자라요."),
        new(2, 3, "새싹", "작은 선택이 땅을 밀어 올렸어요."),
        new(3, 7, "어린잎", "첫 고비를 지나 뿌리가 단단해졌어요."),
        new(4, 14, "푸른 줄기", "반복이 의지가 아니라 환경이 되어가요."),
        new(5, 30, "작은 나무", "한 달의 선택이 새로운 기준을 만들었어요."),
        new(6, 60, "깊은 뿌리", "흔들려도 돌아올 곳을 알고 있어요."),
        new(7, 90, "꽃망울", "당신의 시간이 곧 꽃을 피워요."),
        new(8, 180, "만개", "반년 동안 돌본 삶이 환하게 피었어요."),
        new(9, 365, "생명의 나무", "한 해의 선택이 하나의 세계가 되었어요.")
    ];

    public static readonly string[] Reasons =
    [
        "심심함", "스트레스", "외로움", "인스타·SNS", "영상·웹사이트",
        "꿈", "주변 지인", "술", "피곤함", "습관적인 행동", "기타"
    ];

    public static readonly string[] GroundingMessages =
    [
        "충동은 명령이 아니에요. 10분만 결정을 미뤄보세요.",
        "지금 느끼는 파도는 반드시 내려갑니다. 자리에서 일어나 물 한 잔을 마셔요.",
        "완벽해질 필요는 없어요. 지금 한 번의 선택만 지키면 됩니다.",
        "화면에서 멀어져 문 밖으로 5분만 걸어보세요.",
        "숨을 4초 들이마시고, 6초 내쉬세요. 다섯 번이면 충분해요.",
        "왜 시작했는지 한 문장으로 소리 내어 말해보세요."
    ];

    public static readonly (string Title, string Url, string Kind)[] Resources =
    [
        ("3분 호흡 공간", "https://www.youtube.com/results?search_query=3+minute+breathing+exercise", "호흡"),
        ("집중을 위한 잔잔한 음악", "https://www.youtube.com/results?search_query=calm+focus+music+no+lyrics", "음악"),
        ("10분 걷기 동기부여", "https://www.youtube.com/results?search_query=10+minute+walking+motivation", "영상"),
        ("충동을 파도처럼 바라보기", "https://www.youtube.com/results?search_query=urge+surfing+guided+meditation", "명상")
    ];

    public static GrowthStage StageFor(int days) =>
        Stages.Last(stage => days >= stage.MinimumDays);

    public static string AdviceFor(string reason) => reason switch
    {
        "심심함" => "빈 시간을 미리 채워두세요. 10분 산책·샤워·설거지처럼 시작 장벽이 낮은 행동 세 가지를 적어두면 좋아요.",
        "스트레스" => "스트레스를 없애려 하기보다 몸의 긴장을 먼저 낮춰보세요. 4초 들숨과 6초 날숨을 다섯 번 반복하세요.",
        "외로움" => "혼자 버티는 구조를 바꿔보세요. 믿을 수 있는 사람에게 짧은 안부를 보내거나 사람이 있는 공간으로 이동하세요.",
        "인스타·SNS" or "영상·웹사이트" => "자극에 도달하기 어렵게 만드는 것이 핵심이에요. 로그아웃하고 추천 피드를 끄며 사용 시간을 제한하세요.",
        "꿈" => "꿈은 선택이 아니므로 실패가 아니에요. 수면과 감정만 기록하고 평소 루틴으로 돌아가세요.",
        "주변 지인" => "경계를 미리 한 문장으로 준비하세요. ‘나는 요즘 이 습관을 쉬고 있어’처럼 짧고 설명 없이 말해도 됩니다.",
        "술" => "판단력이 흐려지는 환경을 피하고 음주량을 미리 정하세요. 가능하면 목표가 안정될 때까지 술자리를 줄여보세요.",
        "피곤함" => "의지보다 회복이 먼저예요. 화면을 내려놓고 물·간단한 식사·20분 휴식 중 하나를 선택하세요.",
        _ => "실패 직전의 장소·시간·감정을 한 줄로 남겨보세요. 반복되는 조건 하나를 다음 시도 전에 제거하는 것이 좋습니다."
    };
}
