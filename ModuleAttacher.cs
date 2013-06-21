/*
 * Created by SharpDevelop.
 * User: Bernhard
 * Date: 07.03.2013
 * Time: 03:14
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

//public class PartlessLoader : KSP.Testing.UnitTest
//{
//	public PartlessLoader() : base()
//	{
//		//Called at the first loading screen
//		//When you start the game.
//		MyPlugin.Initialize();
//	}
//}
//
//public static class MyPlugin
//{
//	private static UnityEngine.GameObject MyMonobehaviourObject;
//
//	public static void Initialize()
//	{
//		MyMonobehaviourObject = new UnityEngine.GameObject("ModuleAttacherLoader", new Type[] {typeof(ModuleAttacher)});
//		UnityEngine.GameObject.DontDestroyOnLoad(MyMonobehaviourObject);
//	}
//}

public class AttachmentRestriction
{
	public readonly string property;
	public readonly string comparer;
	public readonly string value;
	
	public AttachmentRestriction(string prop, string comp, string val)
	{
		property = prop;
		comparer = comp;
		value = val;
	}
	
	public bool RestrictionMet(Part part)
	{
		object val;
		var type = part.GetType();
		var prop = type.GetProperty(property);
		
		if (prop != null)
		{
			val = prop.GetValue(part, null);
		}
		else
		{
			val = type.GetField(property).GetValue(part);
		}
		
		switch (val.GetType().ToString())
		{
			case "System.Int32":
				System.Int32 i32v = (System.Int32)val;
				System.Int32 i32t = System.Int32.Parse(value);
				switch (comparer)
				{
						case  ">": return i32v  > i32t;
						case  "<": return i32v  < i32t;
						case "!=": return i32v != i32t;
						case "==": return i32v == i32t;
						case ">=": return i32v >= i32t;
						case "<=": return i32v <= i32t;
						default: ModuleAttacher.print("unknown comparer for " + val.GetType() + ": " + comparer); return false;
				}
				
			default:
				ModuleAttacher.print("unknown value type: " + val.GetType());
				return false;
		}
	}
}

public class Attachment
{
	public readonly string attachTo;
	public readonly string moduleName;
	public readonly List<AttachmentRestriction> restrictions = new List<AttachmentRestriction>();
	
	public Attachment(string attachModule, string attachTarget)
	{
		moduleName = attachModule;
		attachTo = attachTarget;
		ModuleAttacher.print("new attachment: " + moduleName + " for " + attachTo);
		if (attachTo.Contains("["))
		{
			attachTo = attachTarget.Remove(attachTarget.IndexOf("["));
			attachTarget = attachTarget.Substring(attachTarget.IndexOf("[") + 1);
			attachTarget = attachTarget.Remove(attachTarget.IndexOf("]"));
			string[] rests = attachTarget.Split("\"".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			restrictions.Add(new AttachmentRestriction(rests[0], rests[1], rests[2]));
		}
	}
	
	public bool RestrictionsMet(Part part)
	{
		foreach (AttachmentRestriction rest in restrictions)
		{
			if (!rest.RestrictionMet(part))
			{
				return false;
			}
		}
		return true;
	}
}

[KSPAddon(KSPAddon.Startup.Flight, false)]
public class ModuleAttacher : MonoBehaviour
{
	private Vessel last;
	private List<Attachment> attachments;
	
	private void LoadCFG()
	{
		attachments = new List<Attachment>();
		string file = "ModuleAttacher.cfg";
		
		if (KSP.IO.File.Exists<ModuleAttacher>(file))
		{
			string[] ls = KSP.IO.File.ReadAllLines<ModuleAttacher>(file);

			foreach (string l in ls)
			{
				string module = l.Substring(0, l.IndexOf(":"));
				string target = l.Substring(l.IndexOf(":") + 1);
				if (!l.StartsWith("//")) {
					attachments.Add(new Attachment(module, target));
				}
//				string[] pcs = l.Split(':');
//				if (pcs.Length >= 2)
//				{
//					string part = "";
//					for (int i = 1; i < pcs.Length; i++)
//					{
//						part += ((i > 1) ? ":" : "") + pcs[i];
//					}
//					attachments.Add(new Attachment(pcs[0], part));
//				}
			}
			
			print(attachments.Count + " attachments loaded");
		}
		else {
			KSP.IO.File.WriteAllText<ModuleAttacher>("//ModuleToAttach:ModuleToLookFor\n", file);
		}
	}
	
	private bool Attach(Part part)
	{
		bool attached = false;
		List<string> toAttach = new List<string>();
		
//		print("scanning " + part.name + " for needed attachments");
		foreach (PartModule pm in part.Modules)
		{
			foreach (Attachment attach in attachments.FindAll(a => a.attachTo == pm.moduleName && !part.Modules.Contains(a.moduleName)))
			{
				if (attach.RestrictionsMet(part))
				{
					toAttach.Add(attach.moduleName);
				}
			}
		}
		
		foreach (string a in toAttach)
		{
			print("Attaching " + a + " to " + part.name);
			var pm = part.AddModule(a);
			attached = true;
		}
		
		return attached;
	}
	
	public void Update()
	{
		if (attachments == null)
		{
			LoadCFG();
		}
		
		if (!HighLogic.LoadedSceneIsFlight)
		{
			return;
		}
		
		var ship = FlightGlobals.ActiveVessel;
		
//		try {
		if (ship != null && ship.state == Vessel.State.ACTIVE && last != ship) {
			foreach (Part p in ship.Parts) {
				Attach(p);
			}
			last = ship;
		}
//		}
//		catch (Exception ex) {
//			print(ex.Message);
//		}
	}

	public static void print(string message)
	{
		MonoBehaviour.print("ModuleAttacher: " + message);
	}
}
