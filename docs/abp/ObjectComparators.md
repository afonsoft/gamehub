# https://aspnetboilerplate.com/Pages/Documents/ObjectComparators

## ObjectComparators

|     |     |     |
| --- | --- | --- |
| |     |     |
| --- | --- |
|  | × | | search |  |

Custom Search

|     |     |
| --- | --- |
|  | Sort by<br>Relevance<br>Date |

Version

latest (v10.4)v10.3v10.2v10.1v10.0v9.4.2v9.4.1v9.4.0v9.3.0v9.2.0v9.1.3v9.1v9.0v8.4v8.3v8.2v8.1v8.0v7.4v7.3v7.2v7.1v7.0-rc1v7.0v6.6.1v6.6.0v6.5.0v6.4-rc1v6.4.0v6.3.1v6.3v6.2v6.1.1v6.1.0v6.0v5.14v5.13v5.12v5.10.1v5.10v5.9v5.8v5.7v5.6v5.5v5.4v5.3v5.2v5.1.0v5.0.0v4.21v4.20v4.19v4.18v4.17v4.16v4.15v4.14v4.13v4.12v4.11.0v4.10.1v4.10.0v4.9.0v4.8.1v4.8.0v4.7.0v4.6.0v4.5.0v4.4.0v4.3.0v4.2.0v4.1.0v4.0.2v4.0.1v4.0.0v3.9.0v3.8.3v3.8.2v3.8.1v3.8.0v3.7.2v3.7.1v3.7.0v3.6.2v3.6.1v3.6.0v3.5.0v3.4.0v3.3.0v3.2.5v3.2.4v3.2.3v3.2.2v3.2.1v3.2.0v3.1.2v3.1.1v3.1.0v3.0.0-beta3v3.0.0-rc2v3.0.0-beta2v3.0.0-rc1v3.0.0-beta1v3.0.0v2.3.0v2.2.2v2.2.1v2.2.0v2.1.3v2.1.2v2.1.1v2.1.0-beta4v2.1.0-beta3v2.1.0-beta2v2.1.0-beta1v2.1.0v2.0.2v2.0.1v2.0.0-preview4v2.0.0-rc3v2.0.0-preview3v2.0.0-rc2v2.0.0-preview1v2.0.0v2.0.0-rcv1.5.2v1.5.1v1.5.0v1.4.3v1.4.2v1.4.1v1.4.0.0v1.3.1.0v1.3.0.0v1.2.2.0v1.2.1.0v1.2.0.0v1.1.3.0v1.1.1.0v1.1.0.0v1.0.0.0v0.10.3.2Menu

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/ObjectComparators.md)

In this document

## Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/ObjectComparators\#introduction)

**Object Comparator System** is a system which allows you to create comparator for any type of object with any compare type.

### Creating a Comparator [Anchor](https://aspnetboilerplate.com/Pages/Documents/ObjectComparators\#creating-a-comparator)

You can create a comparator for any type of object. Just inherit **ObjectComparatorBase**, **ObjectComparatorBase** or **ObjectComparatorBase<TBaseType, TEnumCompareTypes> where TEnumCompareTypes : Enum**

For example, creating a comparator for string with object and enum:

Copy

```csharp
public class StringObjectComparator : ObjectComparatorBase<string, StringCompareTypes>
{
	protected override bool Compare(string baseObject, string compareObject, StringCompareTypes compareTypes)
	{
		switch (compareTypes)
		{
			case StringCompareTypes.Equals:
				return baseObject == compareObject;
			case StringCompareTypes.Contains:
				return baseObject.Contains(compareObject);
			case StringCompareTypes.StartsWith:
				return baseObject.StartsWith(compareObject);
			case StringCompareTypes.EndsWith:
				return baseObject.EndsWith(compareObject);
			case StringCompareTypes.Null:
				return baseObject.IsNullOrEmpty();
			case StringCompareTypes.NotNull:
				return !baseObject.IsNullOrEmpty();
			default:
				throw new ArgumentOutOfRangeException(nameof(compareTypes), compareTypes, null);
		}
	}
}
```

Another example. Creating a more complex comparator.

Copy

```csharp
public class ObjectComparatorTestClass
{
	public string Prop1 { get; set; }
	public string Prop2 { get; set; }
}

public class ObjectComparatorTestClassObjectComparator : ObjectComparatorBase<ObjectComparatorTestClass>// you can create comparator for any type of object
{
	public override ImmutableList<string> CompareTypes { get; }

	public ObjectComparatorTestClassObjectComparator()
	{
		CompareTypes = (new List<string>()
		{
			"Equal",
			"FirstProp1BiggerThanSecondProp2AsInt"//any compare type you want
		}).ToImmutableList();
	}

	protected override bool Compare(ObjectComparatorTestClass baseObject, ObjectComparatorTestClass compareObject, string compareType)
	{
		if (baseObject == null && compareObject == null)
		{
			return true;
		}

		if (baseObject == null || compareObject == null)
		{
			return false;
		}

		switch (compareType)
		{
			case "Equal":
				return baseObject.Prop1.Equals(compareObject.Prop1) && baseObject.Prop2.Equals(compareObject.Prop2);
			case "FirstProp1BiggerThanSecondProp2AsInt":
				return int.Parse(baseObject.Prop1) > int.Parse(compareObject.Prop2);
			default:
				throw new ArgumentOutOfRangeException(nameof(compareType), compareType, null);
		}
	}
}
```

_Or you can directly inherit **ObjectComparatorBase** and manage everything manually._

### Comparing Objects [Anchor](https://aspnetboilerplate.com/Pages/Documents/ObjectComparators\#comparing-objects)

After you create a comparator for object you can compare that kind of objects with using **IObjectComparatorManager**

Copy

```csharp
public class Test1
{
    private readonly IObjectComparatorManager _objectComparatorManager;
    public Test(IObjectComparatorManager objectComparatorManager)
    {
        _objectComparatorManager = objectComparatorManager;
    }

    public void Compare()
    {
        if(! _objectComparatorManager.HasComparator<string>() || !_objectComparatorManager.CanCompare<string, StringCompareTypes>(StringCompareTypes.StartsWith))
        {
            throw new Exception("Comparator not implemented");
        }
        bool compareResult = _objectComparatorManager.Compare("test", "te", StringCompareTypes.StartsWith);//returns true
    }
}
```

Copy

```csharp
public class Test2
{
    private readonly IObjectComparatorManager _objectComparatorManager;
    public Test(IObjectComparatorManager objectComparatorManager)
    {
        _objectComparatorManager = objectComparatorManager;
    }

    public void Compare()
    {
        if (!_objectComparatorManager.HasComparator<ObjectComparatorTestClass>() || !_objectComparatorManager.CanCompare<ObjectComparatorTestClass>("FirstProp1BiggerThanSecondProp2AsInt"))
        {
            throw new Exception("Comparator not implemented");
        }

        bool compareResult = _objectComparatorManager.Compare(
            new ObjectComparatorTestClass() { Prop1 = "1", Prop2 = "2" },
            new ObjectComparatorTestClass() { Prop1 = "3", Prop2 = "4" },
            "FirstProp1BiggerThanSecondProp2AsInt"
        );
    }
}
```

> Note: You can create multiple comparator which works for same object type with different `CompareTypes`

Twitter Widget Iframe

|     |     |
| --- | --- |
|  |  |