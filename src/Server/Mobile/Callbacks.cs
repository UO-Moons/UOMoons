namespace Server;

public delegate void TargetCallback(Mobile from, object targeted);
public delegate void TargetStateCallback(Mobile from, object targeted, object state);
public delegate void TargetStateCallback<in T>(Mobile from, object targeted, T state);
public delegate void PromptCallback(Mobile from, string text);
public delegate void PromptStateCallback(Mobile from, string text, object state);
public delegate void PromptStateCallback<in T>(Mobile from, string text, T state);
