using Microsoft.EntityFrameworkCore;

using PlantaCoreAPI.Domain.Entities;

namespace PlantaCoreAPI.Infrastructure.Dados;

public class PlantaCoreDbContext : DbContext
{
    public PlantaCoreDbContext(DbContextOptions<PlantaCoreDbContext> opcoes) : base(opcoes) { }
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Planta> Plantas { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Curtida> Curtidas { get; set; } = null!;
    public DbSet<Comentario> Comentarios { get; set; } = null!;
    public DbSet<Notificacao> Notificacoes { get; set; } = null!;
    public DbSet<TokenRefresh> TokensRefresh { get; set; } = null!;
    public DbSet<Comunidade> Comunidades { get; set; } = null!;
    public DbSet<MembroComunidade> MembrosComunidade { get; set; } = null!;
    public DbSet<SolicitacaoSeguir> SolicitacoesSeguir { get; set; } = null!;
    public DbSet<Hashtag> Hashtags { get; set; } = null!;
    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<PalavraChave> PalavrasChave { get; set; } = null!;
    public DbSet<Evento> Eventos { get; set; } = null!;
    public DbSet<EventoParticipante> EventosParticipantes { get; set; } = null!;
    public DbSet<PostSave> PostSaves { get; set; } = null!;
    public DbSet<PostShare> PostShares { get; set; } = null!;
    public DbSet<PostView> PostViews { get; set; } = null!;
    public DbSet<ActivityLog> ActivityLogs { get; set; } = null!;
    public DbSet<Recomendacao> Recomendacoes { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(w =>
        {
            w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.MultipleCollectionIncludeWarning);
            w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.NavigationBaseIncludeIgnored);
        });
        optionsBuilder.UseQueryTrackingBehavior(Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(u => u.Nome).HasColumnName("nome").HasMaxLength(255).IsRequired();
            entity.Property(u => u.Email).HasColumnName("email").IsRequired();
            entity.Property(u => u.SenhaHash).HasColumnName("senha_hash").IsRequired();
            entity.Property(u => u.Biografia).HasColumnName("biografia").HasMaxLength(500);
            entity.Property(u => u.FotoPerfil).HasColumnName("foto_perfil");
            entity.Property(u => u.PerfilPrivado).HasColumnName("perfil_privado").HasDefaultValue(false);
            entity.Property(u => u.EmailConfirmado).HasColumnName("email_confirmado").HasDefaultValue(false);
            entity.Property(u => u.TokenConfirmacaoEmail).HasColumnName("token_confirmacao_email");
            entity.Property(u => u.TokenResetarSenha).HasColumnName("token_resetar_senha");
            entity.Property(u => u.DataTokenResetarSenha).HasColumnName("data_token_resetar");
            entity.Property(u => u.Ativo).HasColumnName("ativo").HasDefaultValue(true);
            entity.Property(u => u.DataCriacao).HasColumnName("data_criacao").ValueGeneratedOnAdd().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(u => u.DataExclusao).HasColumnName("data_exclusao");
            entity.HasMany(u => u.Plantas)
                .WithOne(p => p.Usuario)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_plantas_usuarios");
            entity.HasMany(u => u.Posts)
                .WithOne(p => p.Usuario)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_posts_usuarios");
            entity.HasMany(u => u.Notificacoes)
                .WithOne(n => n.Usuario)
                .HasForeignKey(n => n.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_notificacoes_usuarios");
            entity.HasMany(u => u.Seguidores)
                .WithMany(u => u.Seguindo)
                .UsingEntity(
                    "seguidores",
                    l => l.HasOne(typeof(Usuario)).WithMany().HasForeignKey("seguido_id").OnDelete(DeleteBehavior.Cascade),
                    r => r.HasOne(typeof(Usuario)).WithMany().HasForeignKey("seguidor_id").OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("seguidor_id", "seguido_id");
                        j.ToTable("seguidores");
                    });
            entity.HasMany(u => u.SolicitacoesSeguirRecebidas)
                .WithOne(s => s.Alvo)
                .HasForeignKey(s => s.AlvoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_solicitacoes_seguir_alvo");
            entity.HasMany(u => u.SolicitacoesSeguirEnviadas)
                .WithOne(s => s.Solicitante)
                .HasForeignKey(s => s.SolicitanteId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_solicitacoes_seguir_solicitante");
            entity.HasMany(u => u.ComunidadesParticipantes)
                .WithOne(m => m.Usuario)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_membros_comunidade_usuario");
            entity.HasQueryFilter(u => u.Ativo);
        });
        modelBuilder.Entity<Planta>(entity =>
        {
            entity.ToTable("plantas");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(p => p.UsuarioId).HasColumnName("usuario_id");
            entity.Property(p => p.NomeCientifico).HasColumnName("nome_cientifico").HasMaxLength(500).IsRequired();
            entity.Property(p => p.NomeComum).HasColumnName("nome_comum").HasMaxLength(300);
            entity.Property(p => p.Familia).HasColumnName("familia").HasMaxLength(200);
            entity.Property(p => p.Genero).HasColumnName("genero");
            entity.Property(p => p.Toxica).HasColumnName("toxica").HasDefaultValue(false);
            entity.Property(p => p.DescricaoToxicidade).HasColumnName("descricao_toxicidade");
            entity.Property(p => p.ToxicaAnimais).HasColumnName("toxica_animais").HasDefaultValue(false);
            entity.Property(p => p.DescricaoToxicidadeAnimais).HasColumnName("descricao_toxicidade_animais");
            entity.Property(p => p.ToxicaCriancas).HasColumnName("toxica_criancas").HasDefaultValue(false);
            entity.Property(p => p.DescricaoToxicidadeCriancas).HasColumnName("descricao_toxicidade_criancas");
            entity.Property(p => p.RequisitosLuz).HasColumnName("requisitos_luz");
            entity.Property(p => p.RequisitosAgua).HasColumnName("requisitos_agua");
            entity.Property(p => p.RequisitosTemperatura).HasColumnName("requisitos_temperatura");
            entity.Property(p => p.Cuidados).HasColumnName("cuidados");
            entity.Property(p => p.FotoPlanta).HasColumnName("foto_url");
            entity.Property(p => p.CompartilharLocalizacao).HasColumnName("compartilhar_localizacao").HasDefaultValue(false);
            entity.Property(p => p.Latitude).HasColumnName("latitude");
            entity.Property(p => p.Longitude).HasColumnName("longitude");
            entity.Property(p => p.DadosPlantNet).HasColumnName("dados_plantnet");
            entity.Property(p => p.DataIdentificacao).HasColumnName("data_identificacao");
            entity.Property(p => p.DataCriacao).HasColumnName("data_criacao").ValueGeneratedOnAdd().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(p => p.UsuarioId).HasDatabaseName("ix_plantas_usuario_id");
            entity.HasMany(p => p.Posts)
                .WithOne(po => po.Planta)
                .HasForeignKey(po => po.PlantaId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_posts_plantas")
                .IsRequired(false);
            entity.HasOne(p => p.Usuario)
                .WithMany(u => u.Plantas)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_plantas_usuarios");
            entity.HasQueryFilter(p => p.Usuario != null && p.Usuario.Ativo);
        });
        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("posts");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(p => p.UsuarioId).HasColumnName("usuario_id");
            entity.Property(p => p.PlantaId).HasColumnName("planta_id").IsRequired(false);
            entity.Property(p => p.ComunidadeId).HasColumnName("comunidade_id").IsRequired(false);
            entity.Property(p => p.Conteudo).HasColumnName("conteudo").IsRequired();
            entity.Property(p => p.Ativo).HasColumnName("ativo").HasDefaultValue(true);
            entity.Property(p => p.DataCriacao).HasColumnName("data_criacao").ValueGeneratedOnAdd().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(p => p.DataAtualizacao).HasColumnName("data_atualizacao");
            entity.Property(p => p.DataExclusao).HasColumnName("data_exclusao");
            entity.Ignore(p => p.PontuacaoTotal);
            entity.HasIndex(p => p.UsuarioId).HasDatabaseName("ix_posts_usuario_id");
            entity.HasIndex(p => p.PlantaId).HasDatabaseName("ix_posts_planta_id");
            entity.HasIndex(p => p.ComunidadeId).HasDatabaseName("ix_posts_comunidade_id");
            entity.HasMany(p => p.Comentarios)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_comentarios_posts");
            entity.HasOne(p => p.Usuario)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_posts_usuarios");
            entity.HasOne(p => p.Planta)
                .WithMany(pl => pl.Posts)
                .HasForeignKey(p => p.PlantaId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_posts_plantas")
                .IsRequired(false);
            entity.HasOne(p => p.Comunidade)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.ComunidadeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_posts_comunidades")
                .IsRequired(false);
            entity.HasQueryFilter(p => p.Ativo);
        });
        modelBuilder.Entity<Curtida>(entity =>
        {
            entity.ToTable("curtidas");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(c => c.PostId).HasColumnName("post_id");
            entity.Property(c => c.ComentarioId).HasColumnName("comentario_id");
            entity.Property(c => c.UsuarioId).HasColumnName("usuario_id");
            entity.Property(c => c.DataCriacao).HasColumnName("data_criacao").ValueGeneratedOnAdd().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(c => c.PostId).HasDatabaseName("ix_curtidas_post_id");
            entity.HasIndex(c => c.UsuarioId).HasDatabaseName("ix_curtidas_usuario_id");
            entity.HasIndex(c => new { c.PostId, c.UsuarioId }).IsUnique().HasFilter("post_id IS NOT NULL").HasDatabaseName("ix_curtidas_post_usuario");
            entity.HasIndex(c => new { c.ComentarioId, c.UsuarioId }).IsUnique().HasFilter("comentario_id IS NOT NULL").HasDatabaseName("ix_curtidas_comentario_usuario");
            entity.HasOne(c => c.Post).WithMany(p => p.Curtidas).HasForeignKey(c => c.PostId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_curtidas_posts").IsRequired(false);
            entity.HasOne(c => c.Comentario).WithMany(cm => cm.Curtidas).HasForeignKey(c => c.ComentarioId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_curtidas_comentarios").IsRequired(false);
            entity.HasOne(c => c.Usuario).WithMany().HasForeignKey(c => c.UsuarioId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_curtidas_usuarios").IsRequired(false);
        });
        modelBuilder.Entity<Comentario>(entity =>
        {
            entity.ToTable("comentarios");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(c => c.PostId).HasColumnName("post_id");
            entity.Property(c => c.UsuarioId).HasColumnName("usuario_id");
            entity.Property(c => c.Conteudo).HasColumnName("conteudo").IsRequired();
            entity.Property(c => c.Ativo).HasColumnName("ativo").HasDefaultValue(true);
            entity.Property(c => c.DataCriacao).HasColumnName("data_criacao").ValueGeneratedOnAdd().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(c => c.DataAtualizacao).HasColumnName("data_atualizacao");
            entity.Property(c => c.DataExclusao).HasColumnName("data_exclusao");
            entity.Property(c => c.PontuacaoTotal).HasColumnName("pontuacao_total").HasDefaultValue(0);
            entity.Property(c => c.ComentarioPaiId).HasColumnName("comentario_pai_id");
            entity.HasIndex(c => c.PostId).HasDatabaseName("ix_comentarios_post_id");
            entity.HasIndex(c => c.UsuarioId).HasDatabaseName("ix_comentarios_usuario_id");
            entity.HasOne(c => c.Post).WithMany(p => p.Comentarios).HasForeignKey(c => c.PostId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_comentarios_posts");
            entity.HasOne(c => c.Usuario).WithMany().HasForeignKey(c => c.UsuarioId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_comentarios_usuarios");
            // Backing field para a coleção privada de curtidas
            entity.Metadata.FindNavigation(nameof(Comentario.Curtidas))!
                  .SetField("_curtidas");
            entity.HasQueryFilter(c => c.Ativo);
        });
        modelBuilder.Entity<Notificacao>(entity =>
        {
            entity.ToTable("notificacoes");
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(n => n.UsuarioId).HasColumnName("usuario_id");
            entity.Property(n => n.UsuarioOrigemId).HasColumnName("usuario_origem_id");
            entity.Property(n => n.PlantaId).HasColumnName("planta_id");
            entity.Property(n => n.PostId).HasColumnName("post_id");
            entity.Property(n => n.Tipo).HasColumnName("tipo").HasConversion<string>();
            entity.Property(n => n.Mensagem).HasColumnName("mensagem").IsRequired();
            entity.Property(n => n.Lida).HasColumnName("lida").HasDefaultValue(false);
            entity.Property(n => n.DataCriacao).HasColumnName("data_criacao").ValueGeneratedOnAdd().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(n => n.DataLeitura).HasColumnName("data_leitura");
            entity.Property(n => n.DataDelecao).HasColumnName("data_delecao");
            entity.HasIndex(n => n.UsuarioId).HasDatabaseName("ix_notificacoes_usuario_id");
            entity.HasIndex(n => n.UsuarioOrigemId).HasDatabaseName("ix_notificacoes_usuario_origem_id");
            entity.HasIndex(n => n.PlantaId).HasDatabaseName("ix_notificacoes_planta_id");
            entity.HasIndex(n => n.PostId).HasDatabaseName("ix_notificacoes_post_id");
            entity.HasOne(n => n.Usuario).WithMany(u => u.Notificacoes).HasForeignKey(n => n.UsuarioId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_notificacoes_usuarios");
            entity.HasOne(n => n.UsuarioOrigem).WithMany().HasForeignKey(n => n.UsuarioOrigemId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("fk_notificacoes_usuarios_origem").IsRequired(false);
            entity.HasOne(n => n.Planta).WithMany().HasForeignKey(n => n.PlantaId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("fk_notificacoes_plantas").IsRequired(false);
            entity.HasOne(n => n.Post).WithMany().HasForeignKey(n => n.PostId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("fk_notificacoes_posts").IsRequired(false);
            entity.HasQueryFilter(n => n.Usuario != null && n.Usuario.Ativo);
        });
        modelBuilder.Entity<TokenRefresh>(entity =>
        {
            entity.ToTable("tokens_refresh");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(t => t.UsuarioId).HasColumnName("usuario_id");
            entity.Property(t => t.Token).HasColumnName("token").HasMaxLength(500).IsRequired();
            entity.Property(t => t.DataExpiracao).HasColumnName("data_expiracao");
            entity.Property(t => t.Revogado).HasColumnName("revogado").HasDefaultValue(false);
            entity.Property(t => t.DataCriacao).HasColumnName("data_criacao").ValueGeneratedOnAdd().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(t => t.DataRevogacao).HasColumnName("data_revogacao");
            entity.HasIndex(t => t.UsuarioId).HasDatabaseName("ix_tokens_refresh_usuario_id");
            entity.HasIndex(t => t.Token).IsUnique().HasDatabaseName("ix_tokens_refresh_token");
            entity.HasOne(t => t.Usuario).WithMany().HasForeignKey(t => t.UsuarioId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_tokens_refresh_usuarios");
            entity.HasQueryFilter(t => t.Usuario != null && t.Usuario.Ativo);
        });
        modelBuilder.Entity<Comunidade>(entity =>
        {
            entity.ToTable("comunidades");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(c => c.CriadorId).HasColumnName("criador_id");
            entity.Property(c => c.Nome).HasColumnName("nome").HasMaxLength(100).IsRequired();
            entity.Property(c => c.Descricao).HasColumnName("descricao");
            entity.Property(c => c.FotoComunidade).HasColumnName("foto_comunidade");
            entity.Property(c => c.Ativa).HasColumnName("ativa").HasDefaultValue(true);
            entity.Property(c => c.DataCriacao).HasColumnName("data_criacao").ValueGeneratedOnAdd().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(c => c.Privada).HasColumnName("privada").HasDefaultValue(false);
            entity.HasIndex(c => c.Nome).HasDatabaseName("ix_comunidades_nome");
            entity.HasOne(c => c.Criador).WithMany().HasForeignKey(c => c.CriadorId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_comunidades_criador");
            entity.HasMany(c => c.Membros).WithOne(m => m.Comunidade).HasForeignKey(m => m.ComunidadeId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_membros_comunidade");
            entity.HasQueryFilter(c => c.Ativa);
        });
        modelBuilder.Entity<MembroComunidade>(entity =>
        {
            entity.ToTable("membros_comunidade");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(m => m.ComunidadeId).HasColumnName("comunidade_id");
            entity.Property(m => m.UsuarioId).HasColumnName("usuario_id");
            entity.Property(m => m.EhAdmin).HasColumnName("eh_admin").HasDefaultValue(false);
            entity.Property(m => m.Pendente).HasColumnName("pendente").HasDefaultValue(false);
            entity.Property(m => m.DataEntrada).HasColumnName("data_entrada").ValueGeneratedOnAdd().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(m => new { m.ComunidadeId, m.UsuarioId }).IsUnique().HasDatabaseName("ix_membros_comunidade_unico");
            entity.HasOne(m => m.Comunidade).WithMany(c => c.Membros).HasForeignKey(m => m.ComunidadeId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_membros_comunidade");
            entity.HasOne(m => m.Usuario).WithMany(u => u.ComunidadesParticipantes).HasForeignKey(m => m.UsuarioId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_membros_comunidade_usuario");
            entity.HasQueryFilter(m => m.Comunidade != null && m.Comunidade.Ativa);
        });
        modelBuilder.Entity<SolicitacaoSeguir>(entity =>
        {
            entity.ToTable("solicitacoes_seguir");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(s => s.SolicitanteId).HasColumnName("solicitante_id");
            entity.Property(s => s.AlvoId).HasColumnName("alvo_id");
            entity.Property(s => s.Pendente).HasColumnName("pendente").HasDefaultValue(true);
            entity.Property(s => s.Aceita).HasColumnName("aceita").HasDefaultValue(false);
            entity.Property(s => s.DataSolicitacao).HasColumnName("data_solicitacao").ValueGeneratedOnAdd().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(s => new { s.SolicitanteId, s.AlvoId }).HasDatabaseName("ix_solicitacoes_seguir_par");
            entity.HasOne(s => s.Solicitante).WithMany(u => u.SolicitacoesSeguirEnviadas).HasForeignKey(s => s.SolicitanteId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_solicitacoes_seguir_solicitante");
            entity.HasOne(s => s.Alvo).WithMany(u => u.SolicitacoesSeguirRecebidas).HasForeignKey(s => s.AlvoId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_solicitacoes_seguir_alvo");
            entity.HasQueryFilter(s => s.Solicitante != null && s.Solicitante.Ativo && s.Alvo != null && s.Alvo.Ativo);
        });
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("categorias");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(c => c.Nome).HasColumnName("nome").IsRequired();
            entity.Property(c => c.PostId).HasColumnName("post_id");
            entity.HasOne(c => c.Post)
                .WithMany(p => p.Categorias)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(c => c.Post != null && c.Post.Ativo);
        });
        modelBuilder.Entity<Hashtag>(entity =>
        {
            entity.ToTable("hashtags");
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(h => h.Nome).HasColumnName("nome").IsRequired();
            entity.Property(h => h.PostId).HasColumnName("post_id");
            entity.HasOne(h => h.Post)
                .WithMany(p => p.Hashtags)
                .HasForeignKey(h => h.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(h => h.Post != null && h.Post.Ativo);
        });
        modelBuilder.Entity<PalavraChave>(entity =>
        {
            entity.ToTable("palavras_chave");
            entity.HasKey(pc => pc.Id);
            entity.Property(pc => pc.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(pc => pc.Palavra).HasColumnName("palavra").IsRequired();
            entity.Property(pc => pc.PostId).HasColumnName("post_id");
            entity.HasOne(pc => pc.Post)
                .WithMany(p => p.PalavrasChave)
                .HasForeignKey(pc => pc.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(pc => pc.Post != null && pc.Post.Ativo);
        });
        modelBuilder.Entity<Evento>(entity =>
        {
            entity.ToTable("eventos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.Titulo)
                .HasColumnName("titulo")
                .IsRequired();
            entity.Property(e => e.Descricao)
                .HasColumnName("descricao")
                .IsRequired();
            entity.Property(e => e.Localizacao)
                .HasColumnName("localizacao")
                .IsRequired();
            entity.Property(e => e.DataInicio)
                .HasColumnName("data_inicio");
            entity.Property(e => e.AnfitriaoId)
                .HasColumnName("anfitriao_id");
            entity.HasOne(e => e.Anfitriao)
                .WithMany(u => u.EventosCriados)
                .HasForeignKey(e => e.AnfitriaoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => e.Anfitriao != null && e.Anfitriao.Ativo);
        });
        modelBuilder.Entity<EventoParticipante>(entity =>
        {
            entity.ToTable("eventos_participantes");
            entity.HasKey(ep => new { ep.EventoId, ep.UsuarioId });
            entity.Property(ep => ep.EventoId).HasColumnName("evento_id");
            entity.Property(ep => ep.UsuarioId).HasColumnName("usuario_id");
            entity.HasOne(ep => ep.Evento)
                .WithMany(e => e.Participantes)
                .HasForeignKey(ep => ep.EventoId);
            entity.HasOne(ep => ep.Usuario)
                .WithMany(u => u.EventosParticipando)
                .HasForeignKey(ep => ep.UsuarioId);
            entity.HasQueryFilter(ep => ep.Usuario != null && ep.Usuario.Ativo);
        });
        modelBuilder.Entity<PostSave>(entity =>
        {
            entity.ToTable("post_saves");
            entity.HasKey(ps => ps.Id);
            entity.Property(ps => ps.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(ps => ps.UsuarioId).HasColumnName("usuario_id");
            entity.Property(ps => ps.PostId).HasColumnName("post_id");
            entity.Property(ps => ps.DataCriacao).HasColumnName("data_criacao").ValueGeneratedOnAdd().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(ps => new { ps.UsuarioId, ps.PostId }).IsUnique().HasDatabaseName("ix_postsave_usuario_post");
            entity.HasOne<Usuario>().WithMany().HasForeignKey(ps => ps.UsuarioId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_postsave_usuario");
            entity.HasOne<Post>().WithMany().HasForeignKey(ps => ps.PostId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_postsave_post");
        });
        modelBuilder.Entity<PostShare>(entity =>
        {
            entity.ToTable("post_shares");
            entity.HasKey(ps => ps.Id);
            entity.Property(ps => ps.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(ps => ps.UsuarioId).HasColumnName("usuario_id");
            entity.Property(ps => ps.PostId).HasColumnName("post_id");
            entity.Property(ps => ps.DataCriacao).HasColumnName("data_criacao").ValueGeneratedOnAdd().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(ps => new { ps.UsuarioId, ps.PostId }).IsUnique().HasDatabaseName("ix_postshare_usuario_post");
            entity.HasOne<Usuario>().WithMany().HasForeignKey(ps => ps.UsuarioId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_postshare_usuario");
            entity.HasOne<Post>().WithMany().HasForeignKey(ps => ps.PostId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_postshare_post");
        });
        modelBuilder.Entity<PostView>(entity =>
        {
            entity.ToTable("post_views");
            entity.HasKey(pv => pv.Id);
            entity.Property(pv => pv.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(pv => pv.UsuarioId).HasColumnName("usuario_id");
            entity.Property(pv => pv.PostId).HasColumnName("post_id");
            entity.Property(pv => pv.DataCriacao).HasColumnName("data_criacao").ValueGeneratedOnAdd().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(pv => new { pv.UsuarioId, pv.PostId }).IsUnique().HasDatabaseName("ix_postview_usuario_post");
            entity.HasOne<Usuario>().WithMany().HasForeignKey(pv => pv.UsuarioId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_postview_usuario");
            entity.HasOne<Post>().WithMany().HasForeignKey(pv => pv.PostId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_postview_post");
        });
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("activity_logs");
            entity.HasKey(al => al.Id);
            entity.Property(al => al.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(al => al.UsuarioId).HasColumnName("usuario_id");
            entity.Property(al => al.Tipo).HasColumnName("tipo").IsRequired();
            entity.Property(al => al.EntidadeId).HasColumnName("entidade_id");
            entity.Property(al => al.EntidadeTipo).HasColumnName("entidade_tipo");
            entity.Property(al => al.MetaDados).HasColumnName("meta_dados");
            entity.Property(al => al.DataCriacao).HasColumnName("data_criacao").ValueGeneratedOnAdd().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(al => al.UsuarioId).HasDatabaseName("ix_activitylog_usuario_id");
            entity.HasOne<Usuario>().WithMany().HasForeignKey(al => al.UsuarioId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_activitylog_usuario");
        });
        modelBuilder.Entity<Recomendacao>(entity =>
        {
            entity.ToTable("recomendacoes");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(r => r.NomeComum).HasColumnName("nome_comum");
            entity.Property(r => r.UrlImagem).HasColumnName("url_imagem");
            entity.Property(r => r.Justificativa).HasColumnName("justificativa");
            entity.Property(r => r.Experiencia).HasColumnName("experiencia");
            entity.Property(r => r.Iluminacao).HasColumnName("iluminacao");
            entity.Property(r => r.Regagem).HasColumnName("regagem");
            entity.Property(r => r.Seguranca).HasColumnName("seguranca");
            entity.Property(r => r.Proposito).HasColumnName("proposito");
            entity.HasOne(r => r.Usuario)
                .WithMany(u => u.Recomendacoes)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_recomendacao_usuario");
        });
    }
}
