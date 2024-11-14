namespace DockerRegistryUI.Data;

public readonly struct RepositoryTag : IEquatable<RepositoryTag>
{
    public RepositoryTag(string repositoryName, string tagName)
    {
        RepositoryName = repositoryName;
        TagName = tagName;
    }

    public string RepositoryName { get; }
    public string TagName { get; }

    // Implement IEquatable<T>.Equals method for strongly-typed comparison
    public bool Equals(RepositoryTag other)
    {
        return RepositoryName == other.RepositoryName && TagName == other.TagName;
    }

    // Override Object.Equals to allow equality checks with boxed objects
    public override bool Equals(object obj)
    {
        return obj is RepositoryTag other && Equals(other);
    }

    // Override GetHashCode for use in hash-based collections
    public override int GetHashCode()
    {
        return HashCode.Combine(RepositoryName, TagName);
    }

    // Define equality operator
    public static bool operator ==(RepositoryTag left, RepositoryTag right)
    {
        return left.Equals(right);
    }

    // Define inequality operator
    public static bool operator !=(RepositoryTag left, RepositoryTag right)
    {
        return !left.Equals(right);
    }
}
