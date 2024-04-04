namespace VisitMountVapius.Models;
internal sealed class ArtifactSpotSpawnZone: EntryWithRectangle
{
    public string Type { get; set; } = SObject.artifactSpotQID;

    public RRange Range { get; set; } = new(1, 5);


    private string? id;

    public string Id
    {
        get => this.id ??= this.Range.ToString();
        set => this.id = value;
    }
}
