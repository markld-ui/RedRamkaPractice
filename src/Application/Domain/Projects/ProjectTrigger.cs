namespace Application.Domain.Projects;

public enum ProjectTrigger
{
    StartDevelopment,   // Design -> Development
    SendToQA,           // Development -> QA
    FailQA,             // QA -> Development (parameter: reason)
    PassQA,             // QA -> Delivery
    Release,            // Delivery -> Support
    ReturnToDesign,     // Support -> Design (parameter: reason)
    Archive             // Support -> Archived (parameter: reason)
}
