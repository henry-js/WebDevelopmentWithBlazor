using System.Diagnostics;
using System.Text.Json;
using Data.Models;
using Data.Models.Api;
using Microsoft.Extensions.Options;

namespace Data;

public class BlogApiJsonDirectAccess : IBlogApi
{
    private BlogApiJsonSettings _settings;
    private List<BlogPost>? _blogPosts;
    private List<Category>? _categories;
    private List<Tag>? _tags;

    public BlogApiJsonDirectAccess(IOptions<BlogApiJsonSettings> options)
    {
        _settings = options.Value;
        if (!Directory.Exists(_settings.DataPath))
        {
            Directory.CreateDirectory(_settings.DataPath);
        }
        if (!Directory.Exists($@"{_settings.DataPath}\{_settings.BlogPostsFolder}"))
        {
            Directory.CreateDirectory($@"{_settings.DataPath}\{_settings.BlogPostsFolder}");
        }
        if (!Directory.Exists($@"{_settings.DataPath}\{_settings.CategoriesFolder}"))
        {
            Directory.CreateDirectory($@"{_settings.DataPath}\{_settings.CategoriesFolder}");
        }
        if (!Directory.Exists($@"{_settings.DataPath}\{_settings.TagsFolder}"))
        {
            Directory.CreateDirectory($@"{_settings.DataPath}\{_settings.TagsFolder}");
        }
    }

    private void Load<T>(ref List<T>? list, string folder)
    {
        if (list == null)
            list = new();
        var fullpath = $@"{_settings.DataPath}\{folder}";
        Debug.WriteLine(fullpath);
        foreach (var f in Directory.GetFiles(fullpath))
        {
            var json = File.ReadAllText(f);
            var bp = JsonSerializer.Deserialize<T>(json);
            if (bp != null)
            {
                list.Add(bp);
            }
        }
    }

    private async Task SaveAsync<T>(List<T>? list, string folder, string fileName, T item)
    {
        var filePath = $@"{_settings.DataPath}\{folder}\{fileName}";
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize<T>(item));
        if (list == null)
        {
            list = new();
        }
        if (!list.Contains(item))
        {
            list.Add(item);
        }
    }

    private void DeleteAsync<T>(List<T>? list, string folder, string id)
    {
        var filepath = $@"{_settings.DataPath}\{folder}\{id}.json";
        try
        {
            File.Delete(filepath);
        }
        catch { }
    }

    public Task LoadBlogPostsAsync()
    {
        Load(ref _blogPosts, _settings.BlogPostsFolder);
        return Task.CompletedTask;
    }
    public Task LoadTagsAsync()
    {
        Load(ref _tags, _settings.TagsFolder);
        return Task.CompletedTask;
    }
    public Task LoadCategoriesAsync()
    {
        Load(ref _categories, _settings.CategoriesFolder);
        return Task.CompletedTask;
    }

    public async Task<BlogPost?> SaveBlogPostAsync(BlogPost item)
    {
        if (item.Id == null)
        {
            item.Id = Guid.NewGuid().ToString();
        }
        await SaveAsync(_blogPosts, _settings.BlogPostsFolder, $"{item.Id}.json", item);
        return item;
    }

    public async Task<Category?> SaveCategoryAsync(Category item)
    {
        if (item.Id == null)
        {
            item.Id = Guid.NewGuid().ToString();
        }
        await SaveAsync(_categories, _settings.CategoriesFolder, $"{item.Id}.json", item);
        return item;
    }

    public async Task<Tag?> SaveTagAsync(Tag item)
    {
        if (item.Id == null)
        {
            item.Id = Guid.NewGuid().ToString();
        }
        await SaveAsync(_tags, _settings.TagsFolder, $"{item.Id}.json", item);
        return item;
    }

    public Task DeleteBlogPostAsync(string id)
    {
        DeleteAsync(_blogPosts, _settings.BlogPostsFolder, id);
        if (_blogPosts != null)
        {
            var item = _blogPosts.FirstOrDefault(b => b.Id == id);
            if (item != null)
            {
                _blogPosts.Remove(item);
            }
        }
        return Task.CompletedTask;
    }

    public Task DeleteCategoryAsync(string id)
    {
        DeleteAsync(_categories, _settings.CategoriesFolder, id);
        if (_categories != null)
        {
            var item = _categories.FirstOrDefault(b => b.Id == id);
            if (item != null)
            {
                _categories.Remove(item);
            }
        }
        return Task.CompletedTask;
    }

    public Task DeleteTagAsync(string id)
    {
        DeleteAsync(_tags, _settings.TagsFolder, id);
        if (_tags != null)
        {
            var item = _tags.FirstOrDefault(b => b.Id == id);
            if (item != null)
            {
                _tags.Remove(item);
            }
        }
        return Task.CompletedTask;
    }

    public async Task<BlogPost?> GetBlogPostAsync(string id)
    {
        await LoadBlogPostsAsync();
        if (_blogPosts == null) throw new Exception("Blog posts not found");
        return _blogPosts.SingleOrDefault(b => b.Id == id);
    }

    public async Task<int> GetBlogPostCountAsync()
    {
        await LoadBlogPostsAsync();
        if (_blogPosts == null)
            return 0;
        else
            return _blogPosts.Count;
    }

    public async Task<List<BlogPost>> GetBlogPostsAsync(int numberofposts, int startindex)
    {
        await LoadBlogPostsAsync();
        return _blogPosts ?? new();

    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        await LoadCategoriesAsync();
        return _categories ?? new();
    }
    public async Task<Category?> GetCategoryAsync(string id)
    {
        await LoadCategoriesAsync();
        if (_categories == null)
            throw new Exception("Categories not found");
        return _categories.FirstOrDefault(b => b.Id == id);
    }


    public async Task<List<Tag>> GetTagsAsync()
    {
        await LoadTagsAsync();
        return _tags ?? new();
    }
    public async Task<Tag?> GetTagAsync(string id)
    {
        await LoadTagsAsync();
        if (_tags == null)
            throw new Exception("Tags not found");
        return _tags.FirstOrDefault(b => b.Id == id);
    }

    public Task InvalidateCacheAsync()
    {
        _blogPosts = null;
        _tags = null;
        _categories = null;
        return Task.CompletedTask;
    }

}

public class BlogApiJsonSettings
{
    public string BlogPostsFolder { get; set; } = string.Empty;
    public string CategoriesFolder { get; set; } = string.Empty;
    public string TagsFolder { get; set; } = string.Empty;
    public string DataPath { get; set; } = string.Empty;
}