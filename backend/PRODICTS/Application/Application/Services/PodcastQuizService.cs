using Application.Interface;
using Application.Models.DTOs;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class PodcastQuizService : IPodcastQuizService
{
    private readonly IPodcastQuizRepository _podcastQuizRepository;

    public PodcastQuizService(IPodcastQuizRepository podcastQuizRepository)
    {
        _podcastQuizRepository = podcastQuizRepository;
    }

    // Public endpoints
    public async Task<IEnumerable<PodcastQuizResponseDto>> GetByEpisodeIdAsync(string episodeId)
    {
        var quizzes = await _podcastQuizRepository.GetByEpisodeIdAsync(episodeId);
        return quizzes.Select(MapToResponseDto);
    }

    public async Task<PodcastQuizResponseDto?> GetByIdAsync(string id)
    {
        var quiz = await _podcastQuizRepository.GetByIdAsync(id);
        return quiz != null ? MapToResponseDto(quiz) : null;
    }

    // Admin endpoints
    public async Task<IEnumerable<PodcastQuizResponseDto>> GetAllAsync()
    {
        var quizzes = await _podcastQuizRepository.GetAllAsync();
        return quizzes.Select(MapToResponseDto);
    }

    public async Task<PodcastQuizResponseDto> CreateAsync(CreatePodcastQuizDto dto)
    {
        var quiz = new PodcastQuiz
        {
            PodcastEpisodeId = dto.PodcastEpisodeId,
            Question = dto.Question,
            Answers = dto.Answers,
            CorrectAnswerIndex = dto.CorrectAnswerIndex
        };

        var createdQuiz = await _podcastQuizRepository.CreateAsync(quiz);
        return MapToResponseDto(createdQuiz);
    }

    public async Task<PodcastQuizResponseDto?> UpdateAsync(string id, UpdatePodcastQuizDto dto)
    {
        var existingQuiz = await _podcastQuizRepository.GetByIdAsync(id);
        if (existingQuiz == null)
            return null;

        if (dto.Question != null) existingQuiz.Question = dto.Question;
        if (dto.Answers != null) existingQuiz.Answers = dto.Answers;
        if (dto.CorrectAnswerIndex.HasValue) existingQuiz.CorrectAnswerIndex = dto.CorrectAnswerIndex.Value;

        var updatedQuiz = await _podcastQuizRepository.UpdateAsync(existingQuiz);
        return MapToResponseDto(updatedQuiz);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _podcastQuizRepository.DeleteAsync(id);
    }

    private static PodcastQuizResponseDto MapToResponseDto(PodcastQuiz quiz)
    {
        return new PodcastQuizResponseDto
        {
            Id = quiz.Id,
            PodcastEpisodeId = quiz.PodcastEpisodeId,
            Question = quiz.Question,
            Answers = quiz.Answers,
            CorrectAnswerIndex = quiz.CorrectAnswerIndex
        };
    }
}
