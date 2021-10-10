using Framewerk.Core;
using UnityEngine;

sing UnityEngine;

public class ClassName : Monobehaviour
{
    [SerializeField] SomeClass _reference = default;

    [SerializeField] float _speed = 0.5f;

    [Inject] readonly OtherClass otherReference = default;

    public enum NestedEnum
    {
        First,
        Second
    }

    public class NESTED__CLASS
    {
        public int value;
    }

    public NestedClass anotherReference => _anotherReference;

    public float PublicVariable;

    public event System.Action somethingDidHappenEvent;

    private bool somePrivateProperty => _anotherReference.value != kSomeConstant;

    private NestedClass _anotherReference;

    private const int kSomeConstant = 0;


    protected void Awake()
    {
        // Do Something
    }

    public void NonUnityMethod()
    {
        // Do Something
    }
}