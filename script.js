/* ============================================
   WebExplorer 网站 - 交互脚本
   粒子背景 · 滚动动画 · 导航交互 · 代码复制
   ============================================ */

(function () {
    'use strict';

    /* ============================================
       1. 粒子背景动画
       ============================================ */
    const canvas = document.getElementById('particleCanvas');
    const ctx = canvas.getContext('2d');
    let particles = [];
    let animationId = null;
    let isVisible = true;

    function resizeCanvas() {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
    }

    function createParticles() {
        particles = [];
        const count = Math.min(Math.floor((window.innerWidth * window.innerHeight) / 18000), 70);
        for (let i = 0; i < count; i++) {
            particles.push({
                x: Math.random() * canvas.width,
                y: Math.random() * canvas.height,
                vx: (Math.random() - 0.5) * 0.25,
                vy: (Math.random() - 0.5) * 0.25,
                r: Math.random() * 2 + 0.8,
                opacity: Math.random() * 0.4 + 0.15
            });
        }
    }

    function drawParticles() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        // 绘制连线
        for (let i = 0; i < particles.length; i++) {
            for (let j = i + 1; j < particles.length; j++) {
                const dx = particles[i].x - particles[j].x;
                const dy = particles[i].y - particles[j].y;
                const dist = Math.sqrt(dx * dx + dy * dy);
                if (dist < 130) {
                    ctx.beginPath();
                    ctx.moveTo(particles[i].x, particles[i].y);
                    ctx.lineTo(particles[j].x, particles[j].y);
                    ctx.strokeStyle = 'rgba(0, 120, 212, ' + (0.12 * (1 - dist / 130)) + ')';
                    ctx.lineWidth = 1;
                    ctx.stroke();
                }
            }
        }

        // 绘制粒子
        particles.forEach(function (p) {
            ctx.beginPath();
            ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2);
            ctx.fillStyle = 'rgba(0, 120, 212, ' + p.opacity + ')';
            ctx.fill();

            p.x += p.vx;
            p.y += p.vy;

            if (p.x < 0 || p.x > canvas.width) p.vx *= -1;
            if (p.y < 0 || p.y > canvas.height) p.vy *= -1;
        });

        if (isVisible) {
            animationId = requestAnimationFrame(drawParticles);
        }
    }

    function initParticles() {
        resizeCanvas();
        createParticles();
        if (animationId) cancelAnimationFrame(animationId);
        drawParticles();
    }

    // 尊重 prefers-reduced-motion
    const prefersReduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (!prefersReduced) {
        initParticles();
        let resizeTimer;
        window.addEventListener('resize', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(initParticles, 200);
        });

        // 页面不可见时暂停动画以节省性能
        document.addEventListener('visibilitychange', function () {
            isVisible = !document.hidden;
            if (isVisible && !animationId) {
                drawParticles();
            } else if (!isVisible && animationId) {
                cancelAnimationFrame(animationId);
                animationId = null;
            }
        });
    }

    /* ============================================
       2. 导航栏滚动效果
       ============================================ */
    const navbar = document.getElementById('navbar');
    let lastScrollY = 0;

    window.addEventListener('scroll', function () {
        const scrollY = window.scrollY;
        if (scrollY > 20) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
        lastScrollY = scrollY;
    });

    /* ============================================
       3. 移动端菜单
       ============================================ */
    const navMenuBtn = document.getElementById('navMenuBtn');
    const navMobile = document.getElementById('navMobile');

    if (navMenuBtn) {
        navMenuBtn.addEventListener('click', function () {
            navMobile.classList.toggle('open');
        });

        // 点击移动端链接后关闭菜单
        navMobile.querySelectorAll('.nav-mobile-link').forEach(function (link) {
            link.addEventListener('click', function () {
                navMobile.classList.remove('open');
            });
        });
    }

    /* ============================================
       4. 滚动渐入动画
       ============================================ */
    const animatedElements = document.querySelectorAll(
        '.feature-card, .tech-card, .step-card, .preview-card, .api-row, .section-header'
    );

    animatedElements.forEach(function (el) {
        el.classList.add('animate-on-scroll');
    });

    if ('IntersectionObserver' in window && !prefersReduced) {
        const observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry, index) {
                if (entry.isIntersecting) {
                    // 同组元素错开延迟，营造依次出现的效果
                    const siblings = entry.target.parentElement.children;
                    let delay = 0;
                    for (let i = 0; i < siblings.length; i++) {
                        if (siblings[i] === entry.target) {
                            delay = i * 80;
                            break;
                        }
                    }
                    setTimeout(function () {
                        entry.target.classList.add('visible');
                    }, delay);
                    observer.unobserve(entry.target);
                }
            });
        }, {
            threshold: 0.12,
            rootMargin: '0px 0px -40px 0px'
        });

        animatedElements.forEach(function (el) {
            observer.observe(el);
        });
    } else {
        // 降级：直接显示
        animatedElements.forEach(function (el) {
            el.classList.add('visible');
        });
    }

    /* ============================================
       5. 代码复制
       ============================================ */
    const codeCopyBtn = document.getElementById('codeCopy');
    if (codeCopyBtn) {
        codeCopyBtn.addEventListener('click', function () {
            const codeEl = document.querySelector('.code-block code');
            const text = codeEl ? codeEl.innerText : '';
            if (navigator.clipboard && text) {
                navigator.clipboard.writeText(text).then(function () {
                    codeCopyBtn.textContent = '已复制';
                    codeCopyBtn.classList.add('copied');
                    setTimeout(function () {
                        codeCopyBtn.textContent = '复制';
                        codeCopyBtn.classList.remove('copied');
                    }, 1800);
                }).catch(function () {
                    fallbackCopy(text, codeCopyBtn);
                });
            } else {
                fallbackCopy(text, codeCopyBtn);
            }
        });
    }

    function fallbackCopy(text, btn) {
        const textarea = document.createElement('textarea');
        textarea.value = text;
        textarea.style.position = 'fixed';
        textarea.style.opacity = '0';
        document.body.appendChild(textarea);
        textarea.select();
        try {
            document.execCommand('copy');
            btn.textContent = '已复制';
            btn.classList.add('copied');
            setTimeout(function () {
                btn.textContent = '复制';
                btn.classList.remove('copied');
            }, 1800);
        } catch (e) {
            // 忽略
        }
        document.body.removeChild(textarea);
    }

    /* ============================================
       6. 平滑滚动（兼容旧浏览器）
       ============================================ */
    document.querySelectorAll('a[href^="#"]').forEach(function (anchor) {
        anchor.addEventListener('click', function (e) {
            const targetId = this.getAttribute('href');
            if (targetId === '#' || targetId.length < 2) return;
            const target = document.querySelector(targetId);
            if (target) {
                e.preventDefault();
                const navHeight = navbar.offsetHeight;
                const targetPos = target.getBoundingClientRect().top + window.pageYOffset - navHeight - 12;
                window.scrollTo({
                    top: targetPos,
                    behavior: prefersReduced ? 'auto' : 'smooth'
                });
            }
        });
    });

})();
