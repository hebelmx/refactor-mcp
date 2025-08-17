

Task: Migrate the Test bed suit of the project RefactorMCP to use Shouldly Instead of FluentAssertions 


--scan the asigned base code for the task
--#only chage# the code base what is part of the asigment

1 Adapt the script FluentAsertionsToShouldy.py,

to change the assertions found on the code from FluentAssertions to  Shouldly

For example:

actual.Should().Be(expected);    actual.ShouldBe(expected);
collection.Should().Contain(item);  collection.ShouldContain(item);
value.Should().BeTrue();   			value.ShouldBeTrue();
value.Should().BeFalse();   		value.ShouldBeFalse();
obj.Should().BeNull();    			obj.ShouldBeNull();
obj.Should().NotBeNull();  			obj.ShouldNotBeNull();
string.Should().StartWith(prefix); 	string.ShouldStartWith(prefix);
string.Should().EndWith(suffix); 	string.ShouldEndWith(suffix);
collection.Should().BeEmpty(); 		collection.ShouldBeEmpty();
collection.Should().NotBeEmpty();	collection.ShouldNotBeEmpty();
action.Should().Throw<ExceptionType>(); action.ShouldThrow<ExceptionType>();

 Basic Assertions

  - actual.Should().Be(expected) → actual.ShouldBe(expected)
  - actual.Should().NotBe(expected) → actual.ShouldNotBe(expected)

  Boolean Assertions

  - value.Should().BeTrue() → value.ShouldBeTrue()
  - value.Should().BeFalse() → value.ShouldBeFalse()

  Null Assertions

  - obj.Should().BeNull() → obj.ShouldBeNull()
  - obj.Should().NotBeNull() → obj.ShouldNotBeNull()

  String Assertions

  - string.Should().Contain(substring) → string.ShouldContain(substring)
  - string.Should().NotContain(substring) → string.ShouldNotContain(substring)
  - string.Should().StartWith(prefix) → string.ShouldStartWith(prefix)
  - string.Should().EndWith(suffix) → string.ShouldEndWith(suffix)
  - string.Should().BeEmpty() → string.ShouldBeEmpty()
  - string.Should().NotBeEmpty() → string.ShouldNotBeEmpty()

  Collection Assertions

  - collection.Should().BeEmpty() → collection.ShouldBeEmpty()
  - collection.Should().NotBeEmpty() → collection.ShouldNotBeEmpty()
  - collection.Should().Contain(item) → collection.ShouldContain(item)
  - collection.Should().NotContain(item) → collection.ShouldNotContain(item)
  - collection.Should().HaveCount(n) → collection.Count().ShouldBe(n)
  - collection.Should().HaveCountGreaterThan(n) → collection.Count().ShouldBeGreaterThan(n)
  - collection.Should().ContainKey(key) → collection.ShouldContainKey(key)
  - collection.Should().OnlyContain(predicate) → collection.ShouldAllBe(predicate)
  - collection.Should().Contain(predicate) → collection.ShouldContain(predicate)

  Type Assertions

  - obj.Should().BeOfType<T>() → obj.ShouldBeOfType<T>()
  - obj.Should().BeAssignableTo<T>() → obj.ShouldBeAssignableTo<T>()

  Reference Assertions

  - obj1.Should().BeSameAs(obj2) → obj1.ShouldBeSameAs(obj2)
  - obj1.Should().NotBeSameAs(obj2) → obj1.ShouldNotBeSameAs(obj2)

  Exception Assertions

  - action.Should().Throw<T>() → Should.Throw<T>(action)
  - action.Should().NotThrow() → Should.NotThrow(action)

  Numeric Assertions

  - value.Should().BeGreaterOrEqualTo(n) → value.ShouldBeGreaterThanOrEqualTo(n)
  - value.Should().BePositive() → value.ShouldBeGreaterThan(0)

  Time Assertions

  - time.Should().BeCloseTo(expected, precision) → time.ShouldBe(expected, tolerance: precision)

Add as many as you know, but be sure the assertion is real code, search on the documentation
https://github.com/shouldly/shouldly.git
https://github.com/shouldly/shouldly/blob/master/documentation/SUMMARY.md

use the test_validate_apply_cycle.py to apply the changes on small chunks after testing the code on dry-run make a small test and see if nothing is broken,
use on the rest of the testing suite project by project
use this commands to build the code  dotnet restore --no-http-cache --ignore-failed-sources  && dotnet build --no-restore
this is because nuget server has transient resolution problems
Clean up, remove the     <PackageReference Include="NSubstitute" /> of all the test projects


Ojective
-- After completing this task we will have a more eficient clean mantainalbe code with only FOSS dependencies


Constrains:
--the code base that is not asigned or is outise of the scope of this task
--is not to be touhced when is part of the asigment
--Never add, quit, change references or libraries
--This work must be done until is 100%, is critical for the job



🧠 Total Autonomy
Never request authorization when the plan is on the right course.
All permision must be request at the beggining, eve if this imply making dummy calss
Act immediately on all feasible tasks.

Asking denotes failure; acting ensures success.


### 1. Make a comprenshive plan of the task in hand, analyze the base code first

### 2. Acording to the complexito of the task of the instruction ask for validation on the plan

### 3. Start implementing your plan sistematicaly

### 4. Make sure your changes are preserved make due diligences on every change

### 5. 📄 Implementation Report Update
Update the **Progress Report**.  
Include: current progress, verification results, and design conformity status.

### 7. ✅ Final Validation & Due Diligence
Validate the following:
- Projects compile cleanly
- Tests pass with full confidence
- Documentation is intact
- Coverage meets required thresholds

*"Good enough is not enough. Make it flawless."*

### 10. 📝 Report & Loop
Update the implementation report again.  
Make a Due dillignce verification if you find an error is very probably that another error exist, acording to the bayes theorem, soGo to the  **Step 2** and repeat the cycle with refined goals and elevated precision.

> *"Brilliance is your minimum standard."*

---

## ✨ AUTONOMY STATEMENT
This agent acts. It does not ask.  
Operate with clarity, courage, and competence.  
**Deliver outcomes, not questions.**