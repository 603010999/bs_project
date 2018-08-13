using UnityEngine;
using System.Collections;
using Ink.Runtime;
using UnityEngine.Experimental.UIElements;

public class InkTalkWindow : UIWindowBase
{
    public ReusingScrollRect m_talkItems;
    
    [SerializeField]
    private TextAsset inkJSONAsset;
    private Story story;

    void Awake()
    {
        if (inkJSONAsset == null)
        {
            return;
        }
        
        story = new Story(inkJSONAsset.text);
    }
    

	// When we click the choice button, tell the story to choose that choice!
	private void OnClickChoiceButton (Choice choice) 
	{
		story.ChooseChoiceIndex (choice.index);
		OnRefresh();
	}
    
    
    //UI的初始化请放在这里
    public override void OnOpen()
    {
        m_talkItems.Init(UIEventKey, "");
        //m_talkItems.SetData();
    }

    //请在这里写UI的更新逻辑，当该UI监听的事件触发时，该函数会被调用
    public override void OnRefresh()
    {
		// Read all the content until we can't continue any more
	    while (story.canContinue) 
	    {
		    // Continue gets the next line of the story
		    var text = story.Continue ();
		    
		    // This removes any white space from the text.
		    text.Trim();
		    
			
	    }

	    // Display all the choices, if there are any!
	    if(story.currentChoices.Count > 0) 
	    {
		    for (int i = 0; i < story.currentChoices.Count; i++) 
		    {
				
		    }
	    }
	    // If we've read all the content and there's no choices, the story is finished!
	    else
	    {
			
	    }
    }

    //UI的进入动画
    public override IEnumerator EnterAnim(UIAnimCallBack animComplete, UICallBack callBack, params object[] objs)
    {
        AnimSystem.UguiAlpha(gameObject, 0, 1, callBack:(object[] obj)=>
        {
            StartCoroutine(base.EnterAnim(animComplete, callBack, objs));
        });

        yield return new WaitForEndOfFrame();
    }

    //UI的退出动画
    public override IEnumerator ExitAnim(UIAnimCallBack animComplete, UICallBack callBack, params object[] objs)
    {
        AnimSystem.UguiAlpha(gameObject , null, 0, callBack:(object[] obj) =>
        {
            StartCoroutine(base.ExitAnim(animComplete, callBack, objs));
        });

        yield return new WaitForEndOfFrame();
    }
}